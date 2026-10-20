<script lang="ts">
	let { value = $bindable(1), min = 1, onchange }: { value: number; min?: number; onchange?: (v: number) => void } = $props();
	function set(v: number) {
		value = Math.max(min, Math.floor(Number.isFinite(v) ? v : min));
		onchange?.(value);
	}
</script>

<div class="stepper">
	<button type="button" class="btn secondary" onclick={() => set(value - 1)} aria-label="Menos">−</button>
	<input type="number" {min} {value} onchange={(e) => set(+e.currentTarget.value)} aria-label="Cantidad" />
	<button type="button" class="btn secondary" onclick={() => set(value + 1)} aria-label="Más">+</button>
</div>

<style>
	.stepper { display: inline-flex; gap: 0.25rem; align-items: center; }
	.stepper input { width: 4.2rem; text-align: center; }
	.stepper .btn { padding: 0.4rem 0.7rem; }
</style>
