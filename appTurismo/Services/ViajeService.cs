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
        Task<List<ViajeTurista>> GetTouristTripsAsync();
        Task<ViajeTurista?> GetTouristTripAsync(string grupoId);
        Task<List<Checkpoint>> GetTouristTripCheckpointsAsync(string grupoId);
        Task<List<Models.Supabase.User>> GetAssignedTouristsAsync(string grupoId);
        Task<List<Models.Supabase.User>> GetAvailableTouristsAsync(string grupoId);
        Task AddParticipantAsync(string grupoId, Guid turistaId);
        Task RemoveParticipantAsync(string grupoId, Guid turistaId);
        Task<List<RegistroAsistencia>> GetAttendanceAsync(string grupoId, string checkpointId);
        Task SetAttendanceAsync(string grupoId, string checkpointId, Guid turistaId, bool presente);
        Task RegisterIncidentAsync(IncidenciaParticipante incidencia, bool retirarParticipante);
        Task<List<IncidenciaOperacion>> GetGuideIncidentsAsync();
        Task UpdateIncidentStatusAsync(string incidenciaId, string estado, string notaResolucion);
        Task<List<GrupoTour>> GetActiveGuideTripsAsync();
        Task StartTripAsync(string grupoId);
        Task CompleteTripAsync(string grupoId);
        Task<List<Checkpoint>> GetPlannedTripCheckpointsAsync(string grupoId);
        Task UpdatePlannedTripAsync(GrupoTour grupo, List<Checkpoint> puntos);
        Task CancelTripAsync(string grupoId);
        Task ReactivateTripAsync(string grupoId);
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

        public async Task<List<ViajeTurista>> GetTouristTripsAsync()
        {
            var userId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (!Guid.TryParse(userId, out var turistaId))
            {
                return new List<ViajeTurista>();
            }

            var asignaciones = await _supabaseClient.From<GrupoParticipante>()
                                                    .Where(p => p.IdUsuario == turistaId)
                                                    .Get();
            var resultado = new List<ViajeTurista>();

            foreach (var asignacion in asignaciones.Models)
            {
                var grupoResponse = await _supabaseClient.From<Grupo>()
                                                        .Where(g => g.IdTourGroup == asignacion.IdGrupo)
                                                        .Get();
                var grupo = grupoResponse.Models.FirstOrDefault();
                if (grupo == null) continue;

                Models.Supabase.User? guia = null;
                if (Guid.TryParse(grupo.GuiaId, out var guiaId))
                {
                    var guiaResponse = await _supabaseClient.From<Models.Supabase.User>()
                                                           .Where(u => u.Id_usuario == guiaId)
                                                           .Get();
                    guia = guiaResponse.Models.FirstOrDefault();
                }

                resultado.Add(new ViajeTurista
                {
                    Viaje = ConvertirGrupo(grupo),
                    EstadoParticipacion = asignacion.Estado,
                    NombreGuia = guia == null
                        ? "Guia asignado"
                        : $"{guia.Nombre} {guia.Apellido_paterno}".Trim(),
                    TelefonoGuia = guia?.Telefono ?? string.Empty
                });
            }

            return resultado.OrderByDescending(v => v.FechaInicio).ToList();
        }

        public async Task<ViajeTurista?> GetTouristTripAsync(string grupoId)
        {
            return (await GetTouristTripsAsync())
                .FirstOrDefault(v => v.IdTourGroup == grupoId);
        }

        public async Task<List<Checkpoint>> GetTouristTripCheckpointsAsync(string grupoId)
        {
            if (await GetTouristTripAsync(grupoId) == null)
            {
                throw new InvalidOperationException("Este viaje no esta asignado al turista autenticado.");
            }

            var respuesta = await _supabaseClient.From<Checkpoint>()
                                                 .Where(c => c.IdGrupo == grupoId)
                                                 .Order(c => c.Orden, Supabase.Postgrest.Constants.Ordering.Ascending)
                                                 .Get();
            return respuesta.Models;
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

        public async Task<List<RegistroAsistencia>> GetAttendanceAsync(string grupoId, string checkpointId)
        {
            await EnsureActiveGuideTripAsync(grupoId);
            await EnsureCheckpointInTripAsync(grupoId, checkpointId);

            var asignaciones = await _supabaseClient.From<GrupoParticipante>()
                                                    .Where(p => p.IdGrupo == grupoId)
                                                    .Where(p => p.Estado == "Activo")
                                                    .Get();
            var idsActivos = asignaciones.Models.Select(a => a.IdUsuario).ToHashSet();
            var participantes = (await GetTouristUsersAsync())
                .Where(t => idsActivos.Contains(t.Id_usuario))
                .ToList();
            var registros = await _supabaseClient.From<Asistencia>()
                                                 .Where(a => a.IdCheckpoint == checkpointId)
                                                 .Get();

            var asistenciaPorUsuario = registros.Models
                .GroupBy(a => a.IdUsuario)
                .ToDictionary(g => g.Key, g => g.Last().Presente);

            return participantes.Select(usuario => new RegistroAsistencia
            {
                Usuario = usuario,
                Presente = asistenciaPorUsuario.TryGetValue(usuario.Id_usuario, out var presente) && presente,
                EstadoParticipante = "Activo"
            }).ToList();
        }

        public async Task SetAttendanceAsync(string grupoId, string checkpointId, Guid turistaId, bool presente)
        {
            await EnsureActiveGuideTripAsync(grupoId);
            await EnsureCheckpointInTripAsync(grupoId, checkpointId);

            var participantes = await GetAssignedTouristsAsync(grupoId);
            if (!participantes.Any(p => p.Id_usuario == turistaId))
            {
                throw new InvalidOperationException("El turista no pertenece a este recorrido.");
            }

            var registros = await _supabaseClient.From<Asistencia>()
                                                 .Where(a => a.IdCheckpoint == checkpointId)
                                                 .Where(a => a.IdUsuario == turistaId)
                                                 .Get();

            var existente = registros.Models.FirstOrDefault();
            if (existente == null)
            {
                await _supabaseClient.From<Asistencia>().Insert(new Asistencia
                {
                    Id = Guid.NewGuid().ToString(),
                    IdCheckpoint = checkpointId,
                    IdUsuario = turistaId,
                    Presente = presente
                });
                return;
            }

            await _supabaseClient.From<Asistencia>()
                                 .Where(a => a.Id == existente.Id)
                                 .Set(a => a.Presente, presente)
                                 .Update();
        }

        public async Task RegisterIncidentAsync(IncidenciaParticipante incidencia, bool retirarParticipante)
        {
            await EnsureActiveGuideTripAsync(incidencia.IdGrupo);
            if (!string.IsNullOrWhiteSpace(incidencia.IdCheckpoint))
            {
                await EnsureCheckpointInTripAsync(incidencia.IdGrupo, incidencia.IdCheckpoint);
            }

            var asignacion = await _supabaseClient.From<GrupoParticipante>()
                                                  .Where(p => p.IdGrupo == incidencia.IdGrupo)
                                                  .Where(p => p.IdUsuario == incidencia.IdUsuario)
                                                  .Get();
            var participante = asignacion.Models.FirstOrDefault();
            if (participante == null || !string.Equals(participante.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("El turista ya no esta activo dentro del recorrido.");
            }

            if (_supabaseClient.Auth.CurrentSession?.User == null)
            {
                throw new InvalidOperationException("No hay un guia autenticado.");
            }

            await _supabaseClient.Rpc<string>("registrar_incidencia_guia", new Dictionary<string, object>
            {
                { "p_id_grupo", incidencia.IdGrupo },
                { "p_id_usuario", incidencia.IdUsuario },
                { "p_id_checkpoint", string.IsNullOrWhiteSpace(incidencia.IdCheckpoint) ? null! : incidencia.IdCheckpoint },
                { "p_tipo", incidencia.Tipo },
                { "p_descripcion", string.IsNullOrWhiteSpace(incidencia.Descripcion) ? null! : incidencia.Descripcion },
                { "p_requiere_atencion", incidencia.RequiereAtencion },
                { "p_latitud", incidencia.Latitud ?? (object)null! },
                { "p_longitud", incidencia.Longitud ?? (object)null! },
                { "p_retirar_participante", retirarParticipante }
            });
        }

        public async Task<List<IncidenciaOperacion>> GetGuideIncidentsAsync()
        {
            var guiaId = _supabaseClient.Auth.CurrentSession?.User?.Id;
            if (string.IsNullOrWhiteSpace(guiaId))
            {
                return new List<IncidenciaOperacion>();
            }

            var incidencias = await _supabaseClient.From<IncidenciaParticipante>()
                                                   .Where(i => i.IdGuia == guiaId)
                                                   .Get();
            var viajesGuia = await GetGuideTripsAsync();
            var nombresViajes = viajesGuia.ToDictionary(v => v.IdTourGroup, v => v.Nombre);
            var incidenciasDelGuia = incidencias.Models
                .Where(i => nombresViajes.ContainsKey(i.IdGrupo))
                .OrderByDescending(i => i.CreatedAt)
                .ToList();
            var turistas = await GetTouristUsersAsync();
            var nombresTuristas = turistas.ToDictionary(
                t => t.Id_usuario,
                t => $"{t.Nombre} {t.Apellido_paterno}".Trim());
            var resultado = new List<IncidenciaOperacion>();

            foreach (var incidencia in incidenciasDelGuia)
            {
                var checkpointNombre = string.Empty;
                if (!string.IsNullOrWhiteSpace(incidencia.IdCheckpoint))
                {
                    var checkpoints = await _supabaseClient.From<Checkpoint>()
                                                           .Where(c => c.IdCheckpoint == incidencia.IdCheckpoint)
                                                           .Get();
                    checkpointNombre = checkpoints.Models.FirstOrDefault()?.Nombre ?? string.Empty;
                }

                resultado.Add(new IncidenciaOperacion
                {
                    Incidencia = incidencia,
                    NombreViaje = nombresViajes[incidencia.IdGrupo],
                    NombreTurista = nombresTuristas.TryGetValue(incidencia.IdUsuario, out var nombre) ? nombre : "Turista",
                    NombreCheckpoint = checkpointNombre
                });
            }

            return resultado;
        }

        public async Task UpdateIncidentStatusAsync(string incidenciaId, string estado, string notaResolucion)
        {
            if (_supabaseClient.Auth.CurrentSession?.User == null)
            {
                throw new InvalidOperationException("No hay un guia autenticado.");
            }

            await _supabaseClient.Rpc<object>("actualizar_incidencia_guia", new Dictionary<string, object>
            {
                { "p_id_incidencia", incidenciaId },
                { "p_estado", estado },
                { "p_nota_resolucion", notaResolucion }
            });
        }

        public async Task<List<GrupoTour>> GetActiveGuideTripsAsync()
        {
            var viajes = await GetGuideTripsAsync();
            return viajes.Where(v => string.Equals(v.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(v => v.FechaInicio)
                         .ToList();
        }

        public async Task StartTripAsync(string grupoId)
        {
            await EnsureEditableGuideTripAsync(grupoId);

            await _supabaseClient.From<Grupo>()
                                 .Where(g => g.IdTourGroup == grupoId)
                                 .Set(g => g.Estado, "Activo")
                                 .Update();
        }

        public async Task CompleteTripAsync(string grupoId)
        {
            var viaje = await EnsureGuideTripAsync(grupoId);
            if (!string.Equals(viaje.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Solo un recorrido activo puede finalizarse.");
            }

            var checkpoints = await _supabaseClient.From<Checkpoint>()
                                                   .Where(c => c.IdGrupo == grupoId)
                                                   .Get();

            if (checkpoints.Models.Count == 0)
            {
                throw new InvalidOperationException("El recorrido no tiene checkpoints para validar.");
            }

            if (checkpoints.Models.Any(c => !c.Completado))
            {
                throw new InvalidOperationException("Completa todos los checkpoints antes de finalizar el recorrido.");
            }

            await _supabaseClient.From<Grupo>()
                                 .Where(g => g.IdTourGroup == grupoId)
                                 .Set(g => g.Estado, "Completado")
                                 .Update();
        }

        public async Task<List<Checkpoint>> GetPlannedTripCheckpointsAsync(string grupoId)
        {
            await EnsureEditableGuideTripAsync(grupoId);

            var respuesta = await _supabaseClient.From<Checkpoint>()
                                                 .Where(c => c.IdGrupo == grupoId)
                                                 .Order(c => c.Orden, Supabase.Postgrest.Constants.Ordering.Ascending)
                                                 .Get();
            return respuesta.Models;
        }

        public async Task UpdatePlannedTripAsync(GrupoTour grupo, List<Checkpoint> puntos)
        {
            await EnsureEditableGuideTripAsync(grupo.IdTourGroup);

            await _supabaseClient.From<Grupo>()
                                 .Where(g => g.IdTourGroup == grupo.IdTourGroup)
                                 .Set(g => g.Nombre, grupo.Nombre)
                                 .Set(g => g.Descripcion, grupo.Descripcion ?? string.Empty)
                                 .Set(g => g.PuntoEncuentro, grupo.PuntoEncuentro ?? string.Empty)
                                 .Set(g => g.CupoMaximo, grupo.CupoMaximo ?? 1)
                                 .Set(g => g.FechaInicio, grupo.FechaInicio)
                                 .Update();

            var actuales = await _supabaseClient.From<Checkpoint>()
                                                .Where(c => c.IdGrupo == grupo.IdTourGroup)
                                                .Get();
            var idsConservados = puntos
                .Where(p => !string.IsNullOrWhiteSpace(p.IdCheckpoint))
                .Select(p => p.IdCheckpoint)
                .ToHashSet();

            foreach (var eliminado in actuales.Models.Where(c => !idsConservados.Contains(c.IdCheckpoint)))
            {
                await _supabaseClient.From<Checkpoint>()
                                     .Where(c => c.IdCheckpoint == eliminado.IdCheckpoint)
                                     .Delete();
            }

            for (var indice = 0; indice < puntos.Count; indice++)
            {
                var punto = puntos[indice];
                punto.IdGrupo = grupo.IdTourGroup;
                punto.Orden = indice + 1;
                punto.Completado = false;

                if (string.IsNullOrWhiteSpace(punto.IdCheckpoint))
                {
                    punto.IdCheckpoint = Guid.NewGuid().ToString();
                    await _supabaseClient.From<Checkpoint>().Insert(punto);
                    continue;
                }

                await _supabaseClient.From<Checkpoint>()
                                     .Where(c => c.IdCheckpoint == punto.IdCheckpoint)
                                     .Set(c => c.Nombre, punto.Nombre)
                                     .Set(c => c.Latitud, punto.Latitud)
                                     .Set(c => c.Longitud, punto.Longitud)
                                     .Set(c => c.Orden, punto.Orden)
                                     .Update();
            }
        }

        public async Task CancelTripAsync(string grupoId)
        {
            await EnsureEditableGuideTripAsync(grupoId);

            await _supabaseClient.From<Grupo>()
                                 .Where(g => g.IdTourGroup == grupoId)
                                 .Set(g => g.Estado, "Cancelado")
                                 .Update();
        }

        public async Task ReactivateTripAsync(string grupoId)
        {
            var viaje = await EnsureGuideTripAsync(grupoId);
            if (!string.Equals(viaje.Estado, "Cancelado", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Solo un viaje cancelado puede reactivarse.");
            }

            await _supabaseClient.From<Grupo>()
                                 .Where(g => g.IdTourGroup == grupoId)
                                 .Set(g => g.Estado, "Plan")
                                 .Update();
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

        private async Task<GrupoTour> EnsureActiveGuideTripAsync(string grupoId)
        {
            var viaje = await EnsureGuideTripAsync(grupoId);

            if (!string.Equals(viaje.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La asistencia solo puede registrarse durante un recorrido activo.");
            }

            return viaje;
        }

        private async Task EnsureCheckpointInTripAsync(string grupoId, string checkpointId)
        {
            var respuesta = await _supabaseClient.From<Checkpoint>()
                                                 .Where(c => c.IdGrupo == grupoId)
                                                 .Where(c => c.IdCheckpoint == checkpointId)
                                                 .Get();

            if (!respuesta.Models.Any())
            {
                throw new InvalidOperationException("El checkpoint no pertenece al recorrido seleccionado.");
            }
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

        private static GrupoTour ConvertirGrupo(Grupo grupo) => new()
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
}
