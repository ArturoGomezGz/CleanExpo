// Datos ficticios para desarrollar sin la API de MR. Nada de esto viene de Indigo.
import type { Anuncio, Cliente, Promocion, Stand } from './types';

export const CLIENTE_DEMO: Cliente = {
	idPersona: 1001,
	idEmail: 5001,
	nombre: 'Farmacia San Rafael',
	email: 'demo@expo.test',
	tipo: 'distribuidor',
	sucursal: 'GDL',
	credito: 120000,
	direcciones: [
		{ id: 1, etiqueta: 'Matriz', texto: 'Av. Juárez 120, Guadalajara, Jal.' },
		{ id: 2, etiqueta: 'Sucursal Centro', texto: 'Calle Hidalgo 45, Zapopan, Jal.' }
	],
	transportes: [
		{ id: 1, nombre: 'Entrega MR' },
		{ id: 2, nombre: 'Paquetería (por cobrar)' },
		{ id: 3, nombre: 'Recoge en sucursal' }
	]
};

const anuncio = (titulo: string, texto: string): Anuncio => ({ titulo, texto });

export const STANDS: Stand[] = [
	{ id: 1, nombre: 'Laboratorios Aurora', descripcion: 'Analgésicos y antiinflamatorios de uso diario.', color: '#0b6e4f', anuncios: [anuncio('Compra 10, lleva 1', 'En toda la línea de analgésicos.')] },
	{ id: 2, nombre: 'Farma Norte', descripcion: 'Antibióticos y genéricos intercambiables.', color: '#1d4ed8', anuncios: [anuncio('Genéricos con descuento', 'Descuento escalonado por volumen.')] },
	{ id: 3, nombre: 'DermaVida', descripcion: 'Dermatología y cuidado de la piel.', color: '#b45309', anuncios: [anuncio('Novedad 2026', 'Nueva línea de protección solar.')] },
	{ id: 4, nombre: 'NutriSalud', descripcion: 'Vitaminas, suplementos y nutrición clínica.', color: '#7c3aed', anuncios: [anuncio('Liquidación de vitaminas', 'Existencia limitada.')] },
	{ id: 5, nombre: 'PediaCare', descripcion: 'Línea pediátrica: jarabes, suspensiones y gotas.', color: '#be185d', anuncios: [anuncio('Pack temporada', 'Kit de temporada de frío.')] },
	{ id: 6, nombre: 'Insumos MedPlus', descripcion: 'Material de curación e insumos de consultorio.', color: '#0f766e', anuncios: [anuncio('Envío sin costo', 'En pedidos mayores a $10,000.')] }
];

export const PROMOCIONES: Promocion[] = [
	{ id: 1, standId: 1, titulo: 'Compra 10, lleva 1', tipo: 'piezas', umbral: 10, premio: '1 caja de paracetamol 500 mg', acumulable: true },
	{ id: 2, standId: 2, titulo: 'Pedido mayor a $5,000', tipo: 'monto', umbral: 5000, premio: 'Botiquín de oficina', acumulable: false },
	{ id: 3, standId: 3, titulo: '20 piezas, regalo', tipo: 'piezas', umbral: 20, premio: 'Kit de muestras DermaVida', acumulable: false },
	{ id: 4, standId: 4, titulo: 'Cada $3,000, un frasco gratis', tipo: 'monto', umbral: 3000, premio: '1 frasco de vitamina C', acumulable: true },
	{ id: 5, standId: 5, titulo: 'Compra 6, lleva termómetro', tipo: 'piezas', umbral: 6, premio: 'Termómetro digital', acumulable: false }
];

// Catálogo base por stand: [nombre, presentación, costo, iva, estado]
type Base = [string, string, number, boolean, 'activo' | 'nuevo' | 'liquidacion'];
export const CATALOGO: Record<number, Base[]> = {
	1: [
		['Paracetamol 500 mg', 'Caja 20 tabletas', 18, false, 'activo'],
		['Ibuprofeno 400 mg', 'Caja 20 cápsulas', 42, false, 'activo'],
		['Naproxeno 250 mg', 'Caja 30 tabletas', 55, false, 'activo'],
		['Diclofenaco gel 1%', 'Tubo 60 g', 38, true, 'activo'],
		['Ketorolaco 10 mg', 'Caja 10 tabletas', 47, false, 'nuevo'],
		['Metamizol 500 mg', 'Caja 20 tabletas', 29, false, 'activo']
	],
	2: [
		['Amoxicilina 500 mg', 'Caja 12 cápsulas', 64, false, 'activo'],
		['Azitromicina 500 mg', 'Caja 3 tabletas', 92, false, 'activo'],
		['Ciprofloxacino 500 mg', 'Caja 14 tabletas', 78, false, 'activo'],
		['Cefalexina 500 mg', 'Caja 12 cápsulas', 70, false, 'nuevo'],
		['Omeprazol 20 mg', 'Caja 14 cápsulas', 33, false, 'activo'],
		['Loratadina 10 mg', 'Caja 10 tabletas', 21, false, 'liquidacion']
	],
	3: [
		['Protector solar FPS 50', 'Frasco 120 ml', 135, true, 'nuevo'],
		['Crema hidratante urea 10%', 'Tubo 100 g', 98, true, 'activo'],
		['Clotrimazol crema', 'Tubo 20 g', 44, false, 'activo'],
		['Gel limpiador facial', 'Frasco 200 ml', 110, true, 'activo'],
		['Hidrocortisona 1%', 'Tubo 30 g', 52, false, 'activo'],
		['Shampoo ketoconazol', 'Frasco 120 ml', 120, true, 'liquidacion']
	],
	4: [
		['Vitamina C 1 g', 'Frasco 30 tabletas', 85, true, 'liquidacion'],
		['Complejo B', 'Frasco 60 cápsulas', 96, true, 'activo'],
		['Omega 3', 'Frasco 90 cápsulas', 180, true, 'activo'],
		['Multivitamínico', 'Frasco 100 tabletas', 150, true, 'nuevo'],
		['Hierro + ácido fólico', 'Frasco 60 tabletas', 72, true, 'activo'],
		['Calcio + vitamina D', 'Frasco 60 tabletas', 105, true, 'liquidacion']
	],
	5: [
		['Paracetamol gotas', 'Frasco 15 ml', 31, false, 'activo'],
		['Ambroxol jarabe pediátrico', 'Frasco 120 ml', 48, false, 'activo'],
		['Suero oral sabor fresa', 'Sobre 27.5 g', 12, false, 'activo'],
		['Ibuprofeno suspensión', 'Frasco 120 ml', 56, false, 'nuevo'],
		['Loratadina jarabe', 'Frasco 60 ml', 35, false, 'activo'],
		['Solución nasal salina', 'Frasco 30 ml', 27, true, 'activo']
	],
	6: [
		['Guantes de nitrilo (M)', 'Caja 100 piezas', 120, true, 'activo'],
		['Cubrebocas tricapa', 'Caja 50 piezas', 45, true, 'activo'],
		['Gasa estéril 10x10', 'Paquete 100 piezas', 62, true, 'activo'],
		['Jeringa 5 ml', 'Caja 100 piezas', 135, true, 'nuevo'],
		['Alcohol isopropílico', 'Frasco 500 ml', 38, true, 'activo'],
		['Termómetro digital', 'Pieza', 65, true, 'liquidacion']
	]
};
