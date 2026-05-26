using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class DetalleIncidenciaViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private IncidenciaOperacion? _incidencia;
        private string _notaResolucion = string.Empty;

        public IncidenciaOperacion? Incidencia
        {
            get => _incidencia;
            private set
            {
                _incidencia = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TieneIncidencia));
                OnPropertyChanged(nameof(PuedeAtender));
                OnPropertyChanged(nameof(PuedeCerrar));
            }
        }

        public string NotaResolucion
        {
            get => _notaResolucion;
            set
            {
                _notaResolucion = value;
                OnPropertyChanged();
            }
        }

        public bool TieneIncidencia => Incidencia != null;
        public bool PuedeAtender => string.Equals(Incidencia?.Incidencia.Estado, "Abierta", StringComparison.OrdinalIgnoreCase);
        public bool PuedeCerrar => Incidencia != null && !Incidencia.EsCerrada;

        public ICommand MarcarAtendidaCommand { get; }
        public ICommand CerrarIncidenciaCommand { get; }

        public DetalleIncidenciaViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Detalle de incidencia";
            MarcarAtendidaCommand = new Command(async () => await ActualizarEstadoAsync("Atendida"));
            CerrarIncidenciaCommand = new Command(async () => await ActualizarEstadoAsync("Cerrada"));
        }

        public async Task CargarAsync(string incidenciaId)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(incidenciaId)) return;

            try
            {
                IsBusy = true;
                Incidencia = (await _viajeService.GetGuideIncidentsAsync())
                    .FirstOrDefault(i => i.Incidencia.IdIncidencia == incidenciaId);
                NotaResolucion = Incidencia?.Incidencia.NotaResolucion ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar incidencia: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("Error", "No fue posible consultar la incidencia.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ActualizarEstadoAsync(string estado)
        {
            if (Incidencia == null || IsBusy) return;

            if (string.IsNullOrWhiteSpace(NotaResolucion))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Nota requerida",
                    "Escribe qué acción realizó el guía antes de actualizar la incidencia.",
                    "OK");
                return;
            }

            if (estado == "Cerrada")
            {
                var confirmar = await Shell.Current.DisplayAlertAsync(
                    "Cerrar incidencia",
                    "La incidencia pasara al historial. Confirmas que ya fue resuelta?",
                    "Cerrar",
                    "Cancelar");
                if (!confirmar) return;
            }

            try
            {
                IsBusy = true;
                await _viajeService.UpdateIncidentStatusAsync(
                    Incidencia.Incidencia.IdIncidencia,
                    estado,
                    NotaResolucion.Trim());
                await Shell.Current.DisplayAlertAsync(
                    "Incidencia actualizada",
                    estado == "Cerrada" ? "El caso fue enviado al historial." : "El caso fue marcado como atendido.",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar incidencia: {ex.Message}");
                var mensaje = ex.Message.Contains("actualizar_incidencia_guia", StringComparison.OrdinalIgnoreCase) ||
                              ex.Message.Contains("PGRST202", StringComparison.OrdinalIgnoreCase)
                    ? "Ejecuta nuevamente el script actualizado de incidencias en Supabase para habilitar la atención y cierre."
                    : ex.Message;
                await Shell.Current.DisplayAlertAsync("No se pudo actualizar", mensaje, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
