-- Habilita baja logica de viajes para conservar su historial.
-- Ejecuta este bloque completo una sola vez en Supabase SQL Editor.

alter table public.grupos
  drop constraint if exists grupos_estado_check;

alter table public.grupos
  add constraint grupos_estado_check
  check (estado = any (array['Plan'::text, 'Activo'::text, 'Completado'::text, 'Cancelado'::text]));
