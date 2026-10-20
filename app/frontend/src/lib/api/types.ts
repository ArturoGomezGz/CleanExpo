// Contrato que consume el frontend. Coincide en espíritu con cloud-api / internal-api.openapi.yaml:
// los precios los decide siempre el servidor (Indigo.GetMultiPrecioExpo), nunca el navegador.

export interface Cliente {
	idPersona: number;
	idEmail: number;
	nombre: string;
	email: string;
	tipo: 'mayorista' | 'distribuidor';
	sucursal: 'GDL' | 'QRO' | 'MEX';
	credito: number;
	direcciones: Direccion[];
	transportes: Transporte[];
}

export interface Direccion {
	id: number;
	etiqueta: string;
	texto: string;
}

export interface Transporte {
	id: number;
	nombre: string;
}

export interface Stand {
	id: number;
	nombre: string;
	descripcion: string;
	color: string;
	anuncios: Anuncio[];
}

export interface Anuncio {
	titulo: string;
	texto: string;
}

export interface Producto {
	id: number;
	standId: number;
	nombre: string;
	presentacion: string;
	codigo: string;
	iva: boolean;
	/** Precio unitario para el cliente en sesión (sin IVA). */
	precio: number;
	/** Precio de lista, para mostrar el ahorro. */
	precioLista: number;
	estado: 'activo' | 'nuevo' | 'liquidacion';
}

export interface Promocion {
	id: number;
	standId: number;
	titulo: string;
	/** Condición: comprar al menos `umbral` piezas (tipo piezas) o `umbral` pesos (tipo monto) del stand. */
	tipo: 'piezas' | 'monto';
	umbral: number;
	premio: string;
	acumulable: boolean;
}

export interface LineaCarrito {
	productoId: number;
	cantidad: number;
	/** PorConfirmar del legacy: solo las líneas confirmadas entran al pedido. */
	confirmado: boolean;
}

export interface DatosPedido {
	idDocumento: 1 | 2; // 1 factura, 2 nota
	usoCfdi: string;
	direccionId: number;
	transporteId: number;
	observaciones: string;
}

export type EstadoPedido = 'recibido' | 'sincronizado' | 'parcial' | 'surtido' | 'error';

export interface LineaPedido {
	productoId: number;
	nombre: string;
	cantidad: number;
	unitario: number;
	iva: boolean;
	surtido: number;
}

export interface Pedido {
	id: string;
	folio: string;
	creadoEn: string;
	estado: EstadoPedido;
	datos: DatosPedido;
	lineas: LineaPedido[];
	promociones: string[];
	total: number;
}

export class ApiError extends Error {
	constructor(
		message: string,
		public code: 'sin-credito' | 'carrito-vacio' | 'credenciales' | 'no-encontrado'
	) {
		super(message);
	}
}

export interface ExpoApi {
	login(email: string, password: string): Promise<Cliente>;
	sesion(): Cliente | null;
	logout(): void;
	creditoDisponible(): number;
	listStands(): Promise<Stand[]>;
	getStand(id: number): Promise<Stand | undefined>;
	listProductos(q?: { standId?: number; texto?: string; estado?: Producto['estado'] }): Promise<Producto[]>;
	getProducto(id: number): Promise<Producto | undefined>;
	listPromociones(standId?: number): Promise<Promocion[]>;
	getCarrito(): Promise<LineaCarrito[]>;
	setLinea(productoId: number, cantidad: number): Promise<LineaCarrito[]>;
	confirmarLinea(productoId: number, confirmado: boolean): Promise<LineaCarrito[]>;
	confirmarTodas(confirmado: boolean): Promise<LineaCarrito[]>;
	colocarPedido(datos: DatosPedido): Promise<Pedido>;
	listPedidos(): Promise<Pedido[]>;
	getPedido(id: string): Promise<Pedido | undefined>;
}
