using System.Collections.ObjectModel;
using System.Windows.Input;
using appTurismo.Models;
using appTurismo.Services;

namespace appTurismo.ViewModels
{
    public class AsistenciaCheckpointViewModel : GuiaBaseViewModel
    {
        private readonly IViajeService _viajeService;
        private string _grupoId = string.Empty;
        private string _checkpointId = string.Empty;
        private string _nombreCheckpoint = string.Empty;

        public ObservableCollection<RegistroAsistencia> Registros { get; } = new();
        public ICommand CambiarPresenciaCommand { get; }
        public ICommand RegistrarIncidenciaCommand { get; }

        public string NombreCheckpoint
        {
            get => _nombreCheckpoint;
            private set
            {
                _nombreCheckpoint = value;
                OnPropertyChanged();
            }
        }

        public int TotalParticipantes => Registros.Count;
        public int TotalPresentes => Registros.Count(r => r.Presente);
        public string Resumen => $"Presentes: {TotalPresentes} de {TotalParticipantes}";

        public AsistenciaCheckpointViewModel(IViajeService viajeService, IUserService userService) : base(userService)
        {
            _viajeService = viajeService;
            Title = "Pase de lista";
            CambiarPresenciaCommand = new Command<RegistroAsistencia>(async registro => await CambiarPresenciaAsync(registro));
            RegistrarIncidenciaCommand = new Command<RegistroAsistencia>(async registro => await AbrirRegistroIncidenciaAsync(registro));
        }

        public async Task CargarAsync(string grupoId, string checkpointId, string nombreCheckpoint)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(grupoId) || string.IsNullOrWhiteSpace(checkpointId))
            {
                return;
            }

            _grupoId = grupoId;
            _checkpointId = checkpointId;
            NombreCheckpoint = nombreCheckpoint;
            await CargarRegistrosAsync();
        }

        private async Task CargarRegistrosAsync()
        {
            try
            {
                IsBusy = true;
                var registros = await _viajeService.GetAttendanceAsync(_grupoId, _checkpointId);

                Registros.Clear();
                foreach (var registro in registros)
                {
                    Registros.Add(registro);
                }

                NotificarResumen();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar asistencia: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo cargar", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CambiarPresenciaAsync(RegistroAsistencia? registro)
        {
            if (registro == null || IsBusy) return;

            try
            {
                IsBusy = true;
                await _viajeService.SetAttendanceAsync(
                    _grupoId,
                    _checkpointId,
                    registro.Usuario.Id_usuario,
                    !registro.Presente);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar asistencia: {ex.Message}");
                await Shell.Current.DisplayAlertAsync("No se pudo actualizar", ex.Message, "OK");
                return;
            }
            finally
            {
                IsBusy = false;
            }

            await CargarRegistrosAsync();
        }

        private async Task AbrirRegistroIncidenciaAsync(RegistroAsistencia? registro)
        {
            if (registro == null) return;

            Preferences.Set("IncidenciaTuristaId", registro.Usuario.Id_usuario.ToString());
            Preferences.Set("IncidenciaTuristaNombre", registro.NombreCompleto);
            await Shell.Current.GoToAsync("RegistrarIncidenciaPage");
        }

        private void NotificarResumen()
        {
            OnPropertyChanged(nameof(TotalParticipantes));
            OnPropertyChanged(nameof(TotalPresentes));
            OnPropertyChanged(nameof(Resumen));
        }
    }
}
