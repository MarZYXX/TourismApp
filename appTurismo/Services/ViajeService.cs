using appTurismo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace appTurismo.Services
{
    public interface IViajeService
    {
        Task<List<GrupoTour>> GetGuideTripsAsync(string guiaId);
        Task CreateTripAsync(GrupoTour grupo);
    }

    public class ViajeService : IViajeService
    {
        // 1. SOLUCIÓN AL ERROR CS0103: Inyectamos el cliente de Supabase
        private readonly Supabase.Client _supabaseClient;

        public ViajeService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        // Dejamos tu memoria RAM activa para que tus ejemplos sigan viéndose al instante
        private static List<GrupoTour> _viajesSimulados = new List<GrupoTour>
        {
            new GrupoTour { IdTourGroup = "BOSQUE-001", Estado = "Active", FechaInicio = DateTime.Now },
            new GrupoTour { IdTourGroup = "CASCADA-002", Estado = "Planned", FechaInicio = DateTime.Now.AddDays(2) }
        };

        public async Task<List<GrupoTour>> GetGuideTripsAsync(string guiaId)
        {
            return _viajesSimulados;
        }

        // 2. SOLUCIÓN AL ERROR CS0535: Recibimos GrupoTour y lo transformamos para Supabase
        public async Task CreateTripAsync(GrupoTour grupo)
        {
            // Primero, lo guardamos localmente para que se vea en tu pantalla de inmediato
            grupo.Estado = "Active";
            _viajesSimulados.Add(grupo);

            try
            {
                // Preparamos el paquete exacto para tu base de datos
                var nuevoGrupoSupabase = new Grupo
                {
                    // IMPORTANTE: Tu tabla de Supabase exige que el ID sea un código UUID válido.
                    // Si le mandas "Nájera", Supabase crasheará. Así que le generamos un código real:
                    IdTourGroup = Guid.NewGuid().ToString(),
                    Estado = "Activo",
                    FechaInicio = grupo.FechaInicio
                };

                // Enviamos el registro a la nube
                await _supabaseClient.From<Grupo>().Insert(nuevoGrupoSupabase);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar en Supabase: {ex.Message}");
            }
        }
    }
}