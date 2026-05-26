using appTurismo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace appTurismo.Services
{
    public interface IViajeService
    {
        Task<List<GrupoTour>> GetGuideTripsAsync();
        Task<GrupoTour?> GetGuideTripAsync(string grupoId);
        Task<List<Models.Supabase.User>> GetAssignedTouristsAsync(string grupoId);
        Task<List<Models.Supabase.User>> GetAvailableTouristsAsync(string grupoId);
        Task AddParticipantAsync(string grupoId, Guid turistaId);
        Task RemoveParticipantAsync(string grupoId, Guid turistaId);
        Task CreateTripAsync(GrupoTour grupo, List<Checkpoint> puntos);
    }

    public class ViajeService : IViajeService
    {
        private readonly Supabase.Client _supabaseClient;

        public ViajeService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<GrupoTour>> GetGuideTripsAsync()
        {
            var lista = new List<GrupoTour>();
            try
            {
                var guiaId = _supabaseClient.Auth.CurrentSession?.User?.Id;
                if (string.IsNullOrWhiteSpace(guiaId))
                {
                    return lista;
                }

                var respuesta = await _supabaseClient.From<Grupo>()
                                                     .Where(g => g.GuiaId == guiaId)
                                                     .Get();

                foreach (var g in respuesta.Models)
                {
                    lista.Add(new GrupoTour
                    {
                        IdTourGroup = g.IdTourGroup,
                        IdPaquete = g.IdPaquete,
                        GuiaId = g.GuiaId,
                        Nombre = g.Nombre ?? "Viaje Sin Nombre",
                        Estado = g.Estado,
                        FechaInicio = g.FechaInicio,
                        Descripcion = g.Descripcion,
                        PuntoEncuentro = g.PuntoEncuentro,
                        CupoMaximo = g.CupoMaximo
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al leer viajes: {ex.Message}");
            }
            return lista;
        }

        public async Task<GrupoTour?> GetGuideTripAsync(string grupoId)
        {
            var guiaId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (string.IsNullOrWhiteSpace(guiaId) || string.IsNullOrWhiteSpace(grupoId))
            {
                return null;
            }

            var respuesta = await _supabaseClient.From<Grupo>()
                                                 .Where(g => g.IdTourGroup == grupoId)
                                                 .Where(g => g.GuiaId == guiaId)
                                                 .Get();

            var grupo = respuesta.Models.FirstOrDefault();
            if (grupo == null)
            {
                return null;
            }

            return new GrupoTour
            {
                IdTourGroup = grupo.IdTourGroup,
                IdPaquete = grupo.IdPaquete,
                GuiaId = grupo.GuiaId,
                Nombre = grupo.Nombre ?? "Viaje Sin Nombre",
                Estado = grupo.Estado,
                FechaInicio = grupo.FechaInicio,
                Descripcion = grupo.Descripcion,
                PuntoEncuentro = grupo.PuntoEncuentro,
                CupoMaximo = grupo.CupoMaximo
            };
        }

        public async Task CreateTripAsync(GrupoTour grupo, List<Checkpoint> puntos)
        {
            var guiaId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (string.IsNullOrWhiteSpace(guiaId))
            {
                throw new InvalidOperationException("No hay un guía autenticado para crear el viaje.");
            }

            var nuevoGrupoSupabase = new Grupo
            {
                IdTourGroup = grupo.IdTourGroup,
                Nombre = grupo.Nombre,
                GuiaId = guiaId,
                Estado = "Plan",
                FechaInicio = grupo.FechaInicio,
                Descripcion = grupo.Descripcion,
                PuntoEncuentro = grupo.PuntoEncuentro,
                CupoMaximo = grupo.CupoMaximo
            };

            await _supabaseClient.From<Grupo>().Insert(nuevoGrupoSupabase);

            if (puntos != null && puntos.Count > 0)
            {
                int orden = 1;
                foreach (var p in puntos)
                {
                    p.IdCheckpoint = Guid.NewGuid().ToString();
                    p.IdGrupo = grupo.IdTourGroup;
                    p.Orden = orden;

                    await _supabaseClient.From<Checkpoint>().Insert(p);
                    orden++;
                }
            }
        }

        public async Task<List<Models.Supabase.User>> GetAssignedTouristsAsync(string grupoId)
        {
            await EnsureGuideTripAsync(grupoId);

            var assignments = await _supabaseClient.From<GrupoParticipante>()
                                                   .Where(p => p.IdGrupo == grupoId)
                                                   .Get();

            var participantIds = assignments.Models.Select(p => p.IdUsuario).ToHashSet();
            var turistas = await GetTouristUsersAsync();

            return turistas.Where(t => participantIds.Contains(t.Id_usuario)).ToList();
        }

        public async Task<List<Models.Supabase.User>> GetAvailableTouristsAsync(string grupoId)
        {
            await EnsureEditableGuideTripAsync(grupoId);

            var assignments = await _supabaseClient.From<GrupoParticipante>()
                                                   .Where(p => p.IdGrupo == grupoId)
                                                   .Get();

            var participantIds = assignments.Models.Select(p => p.IdUsuario).ToHashSet();
            var turistas = await GetTouristUsersAsync();

            return turistas.Where(t => !participantIds.Contains(t.Id_usuario)).ToList();
        }

        public async Task AddParticipantAsync(string grupoId, Guid turistaId)
        {
            var viaje = await EnsureEditableGuideTripAsync(grupoId);

            if (viaje.CupoMaximo.HasValue)
            {
                var assignments = await _supabaseClient.From<GrupoParticipante>()
                                                       .Where(p => p.IdGrupo == grupoId)
                                                       .Get();

                if (assignments.Models.Count >= viaje.CupoMaximo.Value)
                {
                    throw new InvalidOperationException("El viaje ya alcanzo su cupo maximo.");
                }
            }

            var disponibles = await GetAvailableTouristsAsync(grupoId);
            if (!disponibles.Any(t => t.Id_usuario == turistaId))
            {
                throw new InvalidOperationException("El turista seleccionado no esta disponible para asignacion.");
            }

            await _supabaseClient.From<GrupoParticipante>().Insert(new GrupoParticipante
            {
                IdGrupo = grupoId,
                IdUsuario = turistaId
            });
        }

        public async Task RemoveParticipantAsync(string grupoId, Guid turistaId)
        {
            await EnsureEditableGuideTripAsync(grupoId);

            await _supabaseClient.From<GrupoParticipante>()
                                 .Where(p => p.IdGrupo == grupoId)
                                 .Where(p => p.IdUsuario == turistaId)
                                 .Delete();
        }

        private async Task<GrupoTour> EnsureEditableGuideTripAsync(string grupoId)
        {
            var viaje = await EnsureGuideTripAsync(grupoId);

            if (!string.Equals(viaje.Estado, "Plan", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Los participantes solo pueden modificarse mientras el viaje esta en Plan.");
            }

            return viaje;
        }

        private async Task<GrupoTour> EnsureGuideTripAsync(string grupoId)
        {
            var viaje = await GetGuideTripAsync(grupoId);
            if (viaje == null)
            {
                throw new InvalidOperationException("El viaje no pertenece al guia autenticado.");
            }

            return viaje;
        }

        private async Task<List<Models.Supabase.User>> GetTouristUsersAsync()
        {
            var roles = await _supabaseClient.From<Models.Supabase.Role>()
                                             .Where(r => r.Nombre == "turista")
                                             .Get();

            var turistaRole = roles.Models.FirstOrDefault();
            if (turistaRole == null)
            {
                return new List<Models.Supabase.User>();
            }

            var response = await _supabaseClient.From<Models.Supabase.User>()
                                                .Where(u => u.Id_rol == turistaRole.Id_rol)
                                                .Get();

            return response.Models;
        }
    }
}
