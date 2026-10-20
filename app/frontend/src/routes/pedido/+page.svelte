<script lang="ts">
	import { goto } from '$app/navigation';
	import { api, ApiError, type DatosPedido } from '#lib/api/index.ts';
	import { cart } from '#lib/state/cart.svelte.ts';
	import { session } from '#lib/state/session.svelte.ts';
	import { money } from '#lib/format.ts';

	const cliente = $derived(session.cliente);
	let paso = $state(1);
	let datos = $state<DatosPedido>({ idDocumento: 1, usoCfdi: 'G03', direccionId: 1, transporteId: 1, observaciones: '' });
	let enviando = $state(false);
	let error = $state('');

	const USOS = [
		['G01', 'Adquisición de mercancías'],
		['G03', 'Gastos en general'],
		['P01', 'Por definir']
	];
	const direccion = $derived(cliente?.direcciones.find((d) => d.id === datos.direccionId));
	const transporte = $derived(cliente?.transportes.find((t) => t.id === datos.transporteId));
	const lineas = $derived(
		cart.lineas.flatMap((l) => {
			const p = cart.productos.get(l.productoId);
			return l.confirmado && p ? [{ l, p }] : [];
		})
	);

	async function confirmar() {
		enviando = true;
		error = '';
		try {
			const pedido = await api.colocarPedido($state.snapshot(datos));
			await cart.cargar();
			session.refrescar();
			goto(`/pedidos/${pedido.id}?nuevo=1`);
		} catch (e) {
			error = e instanceof ApiError ? e.message : 'No fue posible colocar el pedido';
		} finally {
			enviando = false;
		}
	}
</script>

<h1>Colocar pedido</h1>
<ol class="pasos">
	{#each ['Facturación', 'Entrega', 'Resumen'] as t, i}
		<li class:activo={paso === i + 1}>{i + 1}. {t}</li>
	{/each}
</ol>

{#if lineas.length === 0}
	<p>No hay productos confirmados. <a href="/carrito">Volver al carrito</a></p>
{:else if cliente}
	<div class="card">
		{#if paso === 1}
			<div class="field">
				<label for="doc">Documento</label>
				<select id="doc" bind:value={datos.idDocumento}>
					<option value={1}>Factura</option>
					<option value={2}>Nota de remisión</option>
				</select>
			</div>
			<div class="field">
				<label for="cfdi">Uso de CFDI</label>
				<select id="cfdi" bind:value={datos.usoCfdi}>
					{#each USOS as [k, v]}<option value={k}>{k} · {v}</option>{/each}
				</select>
			</div>
			<button class="btn" onclick={() => (paso = 2)}>Siguiente</button>
		{:else if paso === 2}
			<div class="field">
				<label for="dir">Dirección de entrega</label>
				<select id="dir" bind:value={datos.direccionId}>
					{#each cliente.direcciones as d}<option value={d.id}>{d.etiqueta} · {d.texto}</option>{/each}
				</select>
			</div>
			<div class="field">
				<label for="tr">Transporte</label>
				<select id="tr" bind:value={datos.transporteId}>
					{#each cliente.transportes as t}<option value={t.id}>{t.nombre}</option>{/each}
				</select>
			</div>
			<div class="field">
				<label for="obs">Observaciones</label>
				<textarea id="obs" rows="3" bind:value={datos.observaciones}></textarea>
			</div>
			<div class="row"><button class="btn secondary" onclick={() => (paso = 1)}>Atrás</button><button class="btn" onclick={() => (paso = 3)}>Siguiente</button></div>
		{:else}
			<table>
				<tbody>
					{#each lineas as { l, p }}
						<tr><td>{p.nombre} × {l.cantidad}</td><td class="num">{money(p.precio * l.cantidad)}</td></tr>
					{/each}
					<tr><td>Subtotal</td><td class="num">{money(cart.totales.subtotal)}</td></tr>
					<tr><td>IVA</td><td class="num">{money(cart.totales.iva)}</td></tr>
					<tr><th>Total</th><th class="num">{money(cart.totales.total)}</th></tr>
				</tbody>
			</table>
			<p class="muted">
				{datos.idDocumento === 1 ? `Factura (${datos.usoCfdi})` : 'Nota de remisión'} · {transporte?.nombre} · {direccion?.texto}
			</p>
			{#if cart.ganadas.length}<p>🎁 Incluye: {cart.ganadas.map((g) => g.promo.premio).join(', ')}</p>{/if}
			<p class="muted">Los precios se validan de nuevo en el servidor al confirmar.</p>
			{#if error}<p class="error" role="alert">{error}</p>{/if}
			<div class="row">
				<button class="btn secondary" onclick={() => (paso = 2)}>Atrás</button>
				<button class="btn" disabled={enviando} onclick={confirmar}>{enviando ? 'Enviando…' : 'Confirmar pedido'}</button>
			</div>
		{/if}
	</div>
{/if}

<style>
	.pasos { list-style: none; display: flex; gap: 1rem; padding: 0; color: var(--color-muted); }
	.activo { color: var(--color-primary); font-weight: 700; }
</style>
