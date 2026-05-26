-- Diagnostico de incidencias del guia.
-- Es de solo lectura: puede ejecutarse en Supabase SQL Editor sin modificar datos.

-- 1. Verifica que RLS y sus politicas existan en la tabla de incidencias.
select tablename, rowsecurity
from pg_tables
where schemaname = 'public'
  and tablename in ('incidencias_participante', 'grupo_participantes', 'grupos', 'checkpoints');

select tablename, policyname, cmd, roles, qual, with_check
from pg_policies
where schemaname = 'public'
  and tablename in ('incidencias_participante', 'grupo_participantes')
order by tablename, policyname;

-- 2. Verifica que la app pueda invocar las funciones seguras.
select
  p.proname as funcion,
  pg_get_function_identity_arguments(p.oid) as parametros,
  p.prosecdef as security_definer,
  has_function_privilege('authenticated', p.oid, 'EXECUTE') as authenticated_puede_ejecutar
from pg_proc p
join pg_namespace n on n.oid = p.pronamespace
where n.nspname = 'public'
  and p.proname in ('registrar_incidencia_guia', 'actualizar_incidencia_guia', 'cerrar_incidencia_guia')
order by p.proname;

-- 3. Muestra los datos relacionados que deben coincidir al registrar.
-- El recorrido debe tener estado Activo y el participante estado Activo.
select
  g.id_tour_group,
  g.nombre as viaje,
  g.estado as estado_viaje,
  g.guia_id,
  guia.correo_electronico as correo_guia,
  gp.id_usuario as turista_id,
  turista.correo_electronico as correo_turista,
  gp.estado as estado_participante,
  c.id_checkpoint,
  c.nombre as checkpoint
from public.grupos g
left join public.usuarios guia on guia.id_usuario = g.guia_id
left join public.grupo_participantes gp on gp.id_grupo = g.id_tour_group
left join public.usuarios turista on turista.id_usuario = gp.id_usuario
left join public.checkpoints c on c.id_grupo = g.id_tour_group
order by g.created_at desc, turista.correo_electronico, c.orden;

-- 4. Si ya hubiera incidencias, comprueba los registros guardados.
select
  id_incidencia,
  id_grupo,
  id_usuario,
  id_guia,
  id_checkpoint,
  tipo,
  estado,
  created_at
from public.incidencias_participante
order by created_at desc;
