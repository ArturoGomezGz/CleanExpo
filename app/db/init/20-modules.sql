-- Clean Expo — módulos opcionales (monedas, promociones, ruleta). Borrador v0.1
-- Supuesto de Arturo (2026-10-20): siguen vigentes. Se activan con feature flags en `setting`
-- (expo.monedas, expo.promociones, expo.ruleta). Nota: en ExpoMR 7.0.0 ExpoPedidoAgregar ya NO asigna
-- monedas ni promociones al colocar el pedido (bloques comentados): confirmar dónde se otorgan hoy.

-- ---- Monedas (mMonedero, mMotivos, mTransacciones) ----
CREATE TABLE moneda_motivo (
  id_motivo smallint PRIMARY KEY, motivo text NOT NULL, monedas_expo integer NOT NULL DEFAULT 0
  -- 1 volumen venta, 2 dotación, 3 bienvenida, 4 confirmación producto, 5 primer pedido, 6 contacto proveedor
);
CREATE TABLE moneda_saldo (id_persona integer PRIMARY KEY, saldo integer NOT NULL DEFAULT 0 CHECK (saldo >= 0));
CREATE TABLE moneda_transaccion (
  id bigserial PRIMARY KEY,
  origen integer NOT NULL,                       -- 0 = MR, o id de proveedor
  destino integer NOT NULL,                      -- id_persona cliente
  monedas integer NOT NULL CHECK (monedas > 0),
  id_motivo smallint NOT NULL REFERENCES moneda_motivo(id_motivo),
  id_producto smallint, usuario_id bigint,
  saldo_origen integer, saldo_destino integer,
  creado_en timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ON moneda_transaccion (destino, id_motivo);
-- Regla de volumen de venta (hoy en GetMonedas): tramos por total de ventas del cliente, menos lo ya otorgado:
-- 1,000–10k → 1,000 · 10k–20k → 2,500 · 20k–50k → 5,000 · 50k–100k → 10,000 · 100k–500k → 20,000 · >500k → 40,000
CREATE TABLE moneda_tramo_volumen (desde numeric(14,2) PRIMARY KEY, hasta numeric(14,2), monedas integer NOT NULL);

-- ---- Promociones (mPromociones, dPromociones, dPromoCliente, mTipoPromocion) ----
CREATE TABLE promocion_tipo (id smallint PRIMARY KEY, nombre text NOT NULL);  -- 2 acumulable, 3 no acumulable, escalonada
CREATE TABLE promocion (
  id serial PRIMARY KEY,
  proveedor_id integer NOT NULL REFERENCES proveedor(id_persona),
  cliente_id integer REFERENCES cliente(id_persona),   -- NULL = para todos
  id_tipo_promocion smallint NOT NULL REFERENCES promocion_tipo(id),
  id_producto smallint REFERENCES producto(id_producto),
  monto numeric(12,2), piezas integer,                 -- condición por monto o por piezas
  num_promos integer,                                  -- tope total
  activa boolean NOT NULL DEFAULT false,
  usuario_id bigint, creado_en timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE promocion_articulo (                      -- dPromociones: qué regala
  promocion_id integer NOT NULL REFERENCES promocion(id) ON DELETE CASCADE,
  articulo_id bigint NOT NULL REFERENCES articulo(id), cantidad smallint NOT NULL,
  PRIMARY KEY (promocion_id, articulo_id)
);
CREATE TABLE promocion_cliente (                       -- dPromoCliente: lo ganado
  promocion_id integer NOT NULL REFERENCES promocion(id),
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  cantidad smallint NOT NULL,
  PRIMARY KEY (promocion_id, id_persona)
);

-- ---- Ruleta (mRuleta, mPremios, mJuego) ----
CREATE TABLE ruleta (id smallint PRIMARY KEY, nombre text NOT NULL, monedas integer NOT NULL);  -- costo por giro
CREATE TABLE ruleta_premio (
  id smallint PRIMARY KEY, id_ruleta smallint NOT NULL REFERENCES ruleta(id),
  premio text NOT NULL, descripcion text, seccion smallint NOT NULL,
  imagen_id uuid REFERENCES imagen(id), cantidad smallint NOT NULL        -- existencias
);
CREATE TABLE ruleta_giro (
  id bigserial PRIMARY KEY, id_ruleta smallint NOT NULL REFERENCES ruleta(id),
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  premio_id smallint REFERENCES ruleta_premio(id), grados numeric(5,1) NOT NULL,
  creado_en timestamptz NOT NULL DEFAULT now()
);
