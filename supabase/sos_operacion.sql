-- Ejecuta este archivo completo en Supabase SQL Editor para habilitar el
-- envio SOS del turista y la atencion de alertas por el guia.

alter table public.sos enable row level security;

drop policy if exists "Turistas consultan sus solicitudes SOS" on public.sos;
drop policy if exists "Guias consultan SOS de sus recorridos" on public.sos;

create policy "Turistas consultan sus solicitudes SOS"
  on public.sos for select to authenticated
  using (id_usuario = auth.uid());

create policy "Guias consultan SOS de sus recorridos"
  on public.sos for select to authenticated
  using (guia_id = auth.uid());

create or replace function public.registrar_sos_turista(
  p_id_grupo uuid,
  p_latitud double precision,
  p_longitud double precision
) returns uuid language plpgsql security definer set search_path = public as $rpc$
declare
  v_id_sos uuid;
  v_guia_id uuid;
begin
  if auth.uid() is null then
    raise exception 'Sesion requerida.';
  end if;

  if p_latitud not between -90 and 90 or p_longitud not between -180 and 180 then
    raise exception 'Ubicacion invalida.';
  end if;

  select g.guia_id into v_guia_id
    from public.grupos g
   where g.id_tour_group = p_id_grupo
     and g.estado = 'Activo';

  if v_guia_id is null then
    raise exception 'El recorrido no esta activo o no tiene guia.';
  end if;

  if not exists (
    select 1
      from public.grupo_participantes gp
     where gp.id_grupo = p_id_grupo
       and gp.id_usuario = auth.uid()
       and gp.estado = 'Activo'
       and gp.confirmacion_asistencia <> 'No_asistira'
  ) then
    raise exception 'No eres un participante activo de este recorrido.';
  end if;

  if exists (
    select 1
      from public.sos s
     where s.id_grupo_tour = p_id_grupo
       and s.id_usuario = auth.uid()
       and s.estado = 'Activo'
  ) then
    raise exception 'Ya tienes una solicitud SOS activa.';
  end if;

  insert into public.sos (
    id_usuario, id_grupo_tour, latitud, longitud, estado, guia_id
  ) values (
    auth.uid(), p_id_grupo, p_latitud, p_longitud, 'Activo', v_guia_id
  ) returning id_sos into v_id_sos;

  return v_id_sos;
end;
$rpc$;

revoke all on function public.registrar_sos_turista(uuid, double precision, double precision) from public;
grant execute on function public.registrar_sos_turista(uuid, double precision, double precision) to authenticated;

create or replace function public.resolver_sos_guia(
  p_id_sos uuid
) returns void language plpgsql security definer set search_path = public as $rpc$
begin
  if auth.uid() is null then
    raise exception 'Sesion requerida.';
  end if;

  update public.sos
     set estado = 'Resuelto'
   where id_sos = p_id_sos
     and guia_id = auth.uid()
     and estado = 'Activo';

  if not found then
    raise exception 'La solicitud SOS no existe, no pertenece al guia o ya fue resuelta.';
  end if;
end;
$rpc$;

revoke all on function public.resolver_sos_guia(uuid) from public;
grant execute on function public.resolver_sos_guia(uuid) to authenticated;

notify pgrst, 'reload schema';
