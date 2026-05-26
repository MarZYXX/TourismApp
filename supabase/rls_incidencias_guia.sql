-- Ejecuta los tres bloques completos, uno por uno, en Supabase SQL Editor.
-- No ejecutes una seleccion parcial dentro de una funcion.

-- BLOQUE 1: lectura de incidencias propias del guia.
alter table public.incidencias_participante
  add column if not exists nota_resolucion text,
  add column if not exists atendida_at timestamp with time zone,
  add column if not exists cerrada_at timestamp with time zone;

alter table public.incidencias_participante enable row level security;

drop policy if exists "Guias consultan incidencias de sus viajes"
  on public.incidencias_participante;
drop policy if exists "Guias registran incidencias en recorridos activos"
  on public.incidencias_participante;
drop policy if exists "Guias cierran incidencias de sus viajes"
  on public.incidencias_participante;

create policy "Guias consultan incidencias de sus viajes"
  on public.incidencias_participante for select to authenticated
  using (id_guia = auth.uid());

-- BLOQUE 2: registrar incidencia y, opcionalmente, retirar participante.
create or replace function public.registrar_incidencia_guia(
  p_id_grupo uuid, p_id_usuario uuid, p_id_checkpoint uuid, p_tipo text,
  p_descripcion text, p_requiere_atencion boolean, p_latitud double precision,
  p_longitud double precision, p_retirar_participante boolean
) returns uuid language plpgsql security definer set search_path = public as $rpc$
declare
  v_id uuid;
begin
  if auth.uid() is null then raise exception 'Sesion requerida.'; end if;
  if not exists (
    select 1 from public.grupos
    where id_tour_group = p_id_grupo and guia_id = auth.uid() and estado = 'Activo'
  ) then raise exception 'El recorrido no esta activo o no pertenece al guia.'; end if;
  if not exists (
    select 1 from public.grupo_participantes
    where id_grupo = p_id_grupo and id_usuario = p_id_usuario and estado = 'Activo'
  ) then raise exception 'El participante no esta activo en este recorrido.'; end if;
  if p_id_checkpoint is not null and not exists (
    select 1 from public.checkpoints
    where id_checkpoint = p_id_checkpoint and id_grupo = p_id_grupo
  ) then raise exception 'El checkpoint no pertenece al recorrido.'; end if;

  insert into public.incidencias_participante (
    id_grupo, id_usuario, id_checkpoint, id_guia, tipo, descripcion, estado,
    requiere_atencion, latitud, longitud
  ) values (
    p_id_grupo, p_id_usuario, p_id_checkpoint, auth.uid(), p_tipo,
    nullif(btrim(p_descripcion), ''), 'Abierta',
    coalesce(p_requiere_atencion, false), p_latitud, p_longitud
  ) returning id_incidencia into v_id;

  if coalesce(p_retirar_participante, false) then
    update public.grupo_participantes set estado = 'Retirado'
    where id_grupo = p_id_grupo and id_usuario = p_id_usuario and estado = 'Activo';
  end if;
  return v_id;
end;
$rpc$;

revoke all on function public.registrar_incidencia_guia(
  uuid, uuid, uuid, text, text, boolean, double precision, double precision, boolean
) from public;
grant execute on function public.registrar_incidencia_guia(
  uuid, uuid, uuid, text, text, boolean, double precision, double precision, boolean
) to authenticated;

-- BLOQUE 3: atender o cerrar incidencias desde Operacion.
create or replace function public.actualizar_incidencia_guia(
  p_id_incidencia uuid, p_estado text, p_nota_resolucion text
) returns void language plpgsql security definer set search_path = public as $rpc$
begin
  if auth.uid() is null then raise exception 'Sesion requerida.'; end if;
  if p_estado not in ('Atendida', 'Cerrada') then
    raise exception 'Estado de incidencia no permitido.';
  end if;
  if nullif(btrim(p_nota_resolucion), '') is null then
    raise exception 'La nota de resolucion es requerida.';
  end if;

  update public.incidencias_participante
  set estado = p_estado,
      nota_resolucion = btrim(p_nota_resolucion),
      atendida_at = case
        when p_estado = 'Atendida' then coalesce(atendida_at, now())
        else atendida_at
      end,
      cerrada_at = case
        when p_estado = 'Cerrada' then now()
        else cerrada_at
      end
  where id_incidencia = p_id_incidencia
    and id_guia = auth.uid()
    and estado in ('Abierta', 'Atendida');

  if not found then raise exception 'La incidencia no existe, no pertenece al guia o ya fue cerrada.'; end if;
end;
$rpc$;

revoke all on function public.actualizar_incidencia_guia(uuid, text, text) from public;
grant execute on function public.actualizar_incidencia_guia(uuid, text, text) to authenticated;

-- Compatibilidad con versiones anteriores que solo cerraban la incidencia.
create or replace function public.cerrar_incidencia_guia(p_id_incidencia uuid)
returns void language plpgsql security definer set search_path = public as $rpc$
begin
  if auth.uid() is null then raise exception 'Sesion requerida.'; end if;
  update public.incidencias_participante
  set estado = 'Cerrada',
      cerrada_at = now()
  where id_incidencia = p_id_incidencia and id_guia = auth.uid();
  if not found then raise exception 'La incidencia no existe o no pertenece al guia.'; end if;
end;
$rpc$;

revoke all on function public.cerrar_incidencia_guia(uuid) from public;
grant execute on function public.cerrar_incidencia_guia(uuid) to authenticated;
notify pgrst, 'reload schema';
