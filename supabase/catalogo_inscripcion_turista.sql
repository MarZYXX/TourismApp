-- Ejecuta este archivo completo en Supabase SQL Editor.
-- Permite que un turista se inscriba por sí mismo en un viaje disponible.

create or replace function public.inscribirse_viaje_turista(
  p_id_grupo uuid
) returns void language plpgsql security definer set search_path = public as $rpc$
declare
  v_cupo_maximo integer;
  v_participantes integer;
begin
  if auth.uid() is null then
    raise exception 'Sesión requerida.';
  end if;

  if not exists (
    select 1
      from public.usuarios u
      join public.roles r on r.id_rol = u.id_rol
     where u.id_usuario = auth.uid()
       and lower(r.nombre) = 'turista'
  ) then
    raise exception 'Solo una cuenta de turista puede inscribirse en viajes.';
  end if;

  select g.cupo_maximo
    into v_cupo_maximo
    from public.grupos g
   where g.id_tour_group = p_id_grupo
     and g.estado = 'Plan'
     and g.fecha_inicio > now()
   for update;

  if not found then
    raise exception 'El viaje ya no está disponible para inscripción.';
  end if;

  if exists (
    select 1
      from public.grupo_participantes gp
     where gp.id_grupo = p_id_grupo
       and gp.id_usuario = auth.uid()
  ) then
    raise exception 'Ya estás inscrito en este viaje.';
  end if;

  select count(*)
    into v_participantes
    from public.grupo_participantes gp
   where gp.id_grupo = p_id_grupo
     and gp.estado = 'Activo';

  if v_cupo_maximo is not null and v_participantes >= v_cupo_maximo then
    raise exception 'El viaje ya alcanzó su cupo máximo.';
  end if;

  insert into public.grupo_participantes (
    id_grupo,
    id_usuario,
    estado,
    confirmacion_asistencia
  )
  values (
    p_id_grupo,
    auth.uid(),
    'Activo',
    'Pendiente'
  );
end;
$rpc$;

revoke all on function public.inscribirse_viaje_turista(uuid) from public;
grant execute on function public.inscribirse_viaje_turista(uuid) to authenticated;

notify pgrst, 'reload schema';
