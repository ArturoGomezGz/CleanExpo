const mxn = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' });

export const money = (n: number) => mxn.format(n);

export const fechaHora = (iso: string) =>
	new Date(iso).toLocaleString('es-MX', { dateStyle: 'medium', timeStyle: 'short' });
