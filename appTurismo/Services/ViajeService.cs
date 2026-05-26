using appTurismo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace appTurismo.Services
{
    public interface IViajeService
    {
        Task<List<GrupoTour>> GetGuideTripsAsync();
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
                        FechaInicio = g.FechaInicio
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al leer viajes: {ex.Message}");
            }
            return lista;
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
                Estado = "Activo",
                FechaInicio = grupo.FechaInicio
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
    }
}
