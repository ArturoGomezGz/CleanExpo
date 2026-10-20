<script lang="ts">
	import { cart } from '#lib/state/cart.svelte.ts';
	import { session } from '#lib/state/session.svelte.ts';
	import { money } from '#lib/format.ts';
	import Stepper from '#lib/components/Stepper.svelte';

	const filas = $derived(
		cart.lineas.flatMap((l) => {
			const p = cart.productos.get(l.productoId);
			return p ? [{ l, p }] : [];
		})
	);
	const todas = $derived(filas.length > 0 && filas.every(({ l }) => l.confirmado));
	const sinCredito = $derived(cart.totales.total > session.credito);
	const puedeColocar = $derived(cart.totales.piezas > 0 && !sinCredito);
</script>

<h1>Carrito</h1>

{#if !cart.listo}
	<p class="muted">Cargando…</p>
{:else if filas.length === 0}
	<div class="card"><p>Tu carrito está vacío.</p><a class="btn" href="/">Ver stands</a></div>
{:else}
	<div class="card">
		<table>
			<thead>
				<tr>
					<th><input type="checkbox" checked={todas} onchange={(e) => cart.confirmarTodas(e.currentTarget.checked)} aria-label="Confirmar todas" /></th>
					<th>Producto</th><th class="num">Precio</th><th>Cantidad</th><th class="num">Importe</th><th></th>
				</tr>
			</thead>
			<tbody>
				{#each filas as { l, p } (p.id)}
					<tr>
						<td><input type="checkbox" checked={l.confirmado} onchange={(e) => cart.confirmar(p.id, e.currentTarget.checked)} aria-label="Confirmar {p.nombre}" /></td>
						<td><a href="/producto/{p.id}">{p.nombre}</a><div class="muted">{p.presentacion}</div></td>
						<td class="num">{money(p.precio)}</td>
						<td><Stepper value={l.cantidad} onchange={(v) => cart.set(p.id, v)} /></td>
						<td class="num">{money(p.precio * l.cantidad)}</td>
						<td><button class="btn danger" onclick={() => cart.set(p.id, 0)} aria-label="Quitar {p.nombre}">Quitar</button></td>
					</tr>
				{/each}
			</tbody>
		</table>
		<p class="muted">Solo los productos confirmados entran al pedido.</p>
	</div>

	{#if cart.ganadas.length}
		<div class="card promo">
			<strong>🎁 Promociones ganadas</strong>
			<ul>{#each cart.ganadas as g}<li>{g.veces > 1 ? `${g.veces} × ` : ''}{g.promo.premio} <span class="muted">({g.promo.titulo})</span></li>{/each}</ul>
		</div>
	{/if}

	<div class="card resumen">
		<div class="row spread"><span>Subtotal</span><span>{money(cart.totales.subtotal)}</span></div>
		<div class="row spread"><span>IVA</span><span>{money(cart.totales.iva)}</span></div>
		<div class="row spread"><strong>Total</strong><strong>{money(cart.totales.total)}</strong></div>
		<div class="row spread muted"><span>Crédito disponible</span><span>{money(session.credito)}</span></div>
		{#if sinCredito}<p class="error" role="alert">Revisa si tienes crédito suficiente.</p>{/if}
		{#if cart.totales.piezas === 0}<p class="error">Revisa si tienes productos confirmados en tu carrito.</p>{/if}
		<a class="btn" class:disabled={!puedeColocar} href={puedeColocar ? '/pedido' : undefined} aria-disabled={!puedeColocar}>Continuar con el pedido</a>
	</div>
{/if}

<style>
	.promo, .resumen { margin-top: 1rem; }
	.resumen { max-width: 420px; margin-left: auto; }
	.disabled { opacity: 0.5; pointer-events: none; }
</style>
