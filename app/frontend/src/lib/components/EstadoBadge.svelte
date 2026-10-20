<script lang="ts">
	import type { EstadoPedido } from '#lib/api/index.ts';
	let { estado }: { estado: EstadoPedido } = $props();
	const info: Record<EstadoPedido, [string, string]> = {
		recibido: ['Recibido', 'warn'],
		sincronizado: ['Enviado a MR', 'warn'],
		parcial: ['Surtido parcial', 'warn'],
		surtido: ['Surtido', 'ok'],
		error: ['Con error', 'err']
	};
</script>

<span class="badge {info[estado][1]}">{info[estado][0]}</span>
