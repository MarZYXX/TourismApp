# Guía de presentación de TourismApp

## Preparación

- Usa una cuenta con rol `guia` y una cuenta con rol `turista` creadas desde la aplicación.
- Verifica que los scripts SQL de catálogo/inscripción, confirmación de asistencia, incidencias y SOS ya estén ejecutados en Supabase.
- Activa ubicación e internet en el dispositivo que enviará un SOS.
- Antes de iniciar, deja creado un viaje en estado `Plan` con fecha, punto de encuentro, cupo y al menos tres checkpoints.

## Flujo recomendado de demostración

1. Inicia sesión como guía y muestra `Mis Viajes`: filtros de planificados, activos, finalizados y cancelados.
2. Edita el viaje planificado: cambia la información general o un checkpoint y guarda.
3. Inicia sesión como turista: abre `Explorar`, encuentra el viaje disponible y pulsa `Unirme a este viaje`.
4. Abre `Mis Viajes`, consulta el recorrido inscrito, confirma asistencia y amplía el mapa de la ruta.
5. Regresa al guía y muestra la operación: asistencia y avance por checkpoints.
6. Desde turista, envía un SOS con ubicación.
7. Desde guía, abre la ubicación del SOS y resuélvelo; enseña el historial resuelto.
8. Registra una incidencia de participante desde guía, atiéndela y muéstrala en el historial.
9. Completa todos los checkpoints y finaliza el recorrido.
10. Vuelve a `Mis Viajes` para mostrar el viaje en `Finalizados` y abre el perfil del guía o turista.

## Datos de ejemplo

- Viaje: `Ruta Demo Centro`
- Punto de encuentro: `Plaza principal`
- Checkpoints: `Inicio`, `Mirador`, `Punto final`
- Incidencia: `Malestar` con nota breve de atención

No almacenes contraseñas reales en este archivo ni en el repositorio.
