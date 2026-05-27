alter table public.grupos
  drop constraint if exists grupos_estado_check;

alter table public.grupos
  add constraint grupos_estado_check
  check (estado = any (array['Plan'::text, 'Activo'::text, 'Completado'::text, 'Cancelado'::text]));
