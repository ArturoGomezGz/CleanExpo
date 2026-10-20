<script lang="ts">
	import { api } from '#lib/api/index.ts';
	import { fechaHora, money } from '#lib/format.ts';
	import EstadoBadge from '#lib/components/EstadoBadge.svelte';

	let pedidos = $state<Awaited<ReturnType<typeof api.listPedidos>> | null>(null);
	$effect(() => {
		const cargar = () => api.listPedidos().then((p) => (pedidos = p));
		cargar();
		const t = setInterval(cargar, 3000);
		return () => clearInterval(t);
	});
</script>

<h1>Mis pedidos</h1>
{#if pedidos === null}
	<p class="muted">Cargando…</p>
{:else if pedidos.length === 0}
	<div class="card"><p>Aún no has colocado pedidos.</p><a class="btn" href="/">Ver stands</a></div>
{:else}
	<div class="card">
		<table>
			<thead><tr><th>Folio</th><th>Fecha</th><th>Estado</th><th class="num">Total</th></tr></thead>
			<tbody>
				{#each pedidos as p (p.id)}
					<tr>
						<td><a href="/pedidos/{p.id}">{p.folio}</a></td>
						<td>{fechaHora(p.creadoEn)}</td>
						<td><EstadoBadge estado={p.estado} /></td>
						<td class="num">{money(p.total)}</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
{/if}
