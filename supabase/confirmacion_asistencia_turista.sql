alter table public.grupo_participantes
  add column if not exists confirmacion_asistencia text not null default 'Pendiente';

alter table public.grupo_participantes
  drop constraint if exists grupo_participantes_confirmacion_asistencia_check;

alter table public.grupo_participantes
  add constraint grupo_participantes_confirmacion_asistencia_check
  check (confirmacion_asistencia in ('Pendiente', 'Confirmado', 'No_asistira'));

create or replace function public.confirmar_asistencia_turista(
  p_id_grupo uuid,
  p_confirmacion text
) returns void language plpgsql security definer set search_path = public as $rpc$
begin
  if auth.uid() is null then
    raise exception 'Sesion requerida.';
  end if;

  if p_confirmacion not in ('Confirmado', 'No_asistira') then
    raise exception 'Confirmacion de asistencia no permitida.';
  end if;

  if not exists (
    select 1
     from public.grupos g
     where g.id_tour_group = p_id_grupo
       and g.estado in ('Plan', 'Activo')
  ) then
    raise exception 'Solo puedes confirmar asistencia en un viaje planificado o en recorrido.';
  end if;

  update public.grupo_participantes
     set confirmacion_asistencia = p_confirmacion
   where id_grupo = p_id_grupo
     and id_usuario = auth.uid()
     and estado = 'Activo';

  if not found then
    raise exception 'No estas asignado como participante activo de este viaje.';
  end if;
end;
$rpc$;

revoke all on function public.confirmar_asistencia_turista(uuid, text) from public;
grant execute on function public.confirmar_asistencia_turista(uuid, text) to authenticated;

notify pgrst, 'reload schema';
