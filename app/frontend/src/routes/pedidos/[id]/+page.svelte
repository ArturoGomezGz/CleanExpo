<script lang="ts">
	import { page } from '$app/state';
	import { api, type Pedido } from '#lib/api/index.ts';
	import { totalesPedido } from '#lib/cart-math.ts';
	import { fechaHora, money } from '#lib/format.ts';
	import EstadoBadge from '#lib/components/EstadoBadge.svelte';

	let pedido = $state<Pedido | null | undefined>(null);
	$effect(() => {
		const id = page.params.id!;
		const cargar = () => api.getPedido(id).then((p) => (pedido = p));
		cargar();
		const t = setInterval(cargar, 2000);
		return () => clearInterval(t);
	});
	const t = $derived(pedido ? totalesPedido(pedido.lineas) : null);
</script>

<p><a href="/pedidos">← Mis pedidos</a></p>
{#if pedido === null}
	<p class="muted">Cargando…</p>
{:else if pedido === undefined}
	<p>Pedido no encontrado.</p>
{:else}
	{#if page.url.searchParams.get('nuevo')}
		<div class="card ok" role="status">✅ Pedido recibido. Lo estamos enviando a Medicinas Rosario; no necesitas esperar en esta pantalla.</div>
	{/if}
	<h1>{pedido.folio} <EstadoBadge estado={pedido.estado} /></h1>
	<p class="muted">{fechaHora(pedido.creadoEn)}</p>
	<div class="card">
		<table>
			<thead><tr><th>Producto</th><th class="num">Pedido</th><th class="num">Surtido</th><th class="num">Importe</th></tr></thead>
			<tbody>
				{#each pedido.lineas as l}
					<tr>
						<td>{l.nombre}</td><td class="num">{l.cantidad}</td><td class="num">{l.surtido}</td>
						<td class="num">{money(l.cantidad * l.unitario)}</td>
					</tr>
				{/each}
			</tbody>
		</table>
		{#if t}<p class="num"><strong>Total {money(t.total)}</strong> <span class="muted">(IVA {money(t.iva)})</span></p>{/if}
		{#if pedido.promociones.length}<p>🎁 Promociones: {pedido.promociones.join(', ')}</p>{/if}
	</div>
{/if}

<style>
	.ok { border-color: var(--color-ok); margin-bottom: 1rem; }
	p.num { text-align: right; }
</style>
