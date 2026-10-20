-- Clean Expo — datos que necesita la tienda (auth, catálogo por cliente, carrito, pedidos con folio). Borrador v0.1

ALTER TABLE cliente   ADD COLUMN tipo_cliente smallint NOT NULL DEFAULT 0;   -- Indigo: 0 mayorista, 2 distribuidor
ALTER TABLE producto  ADD COLUMN precio_lista numeric(12,2);                 -- precio de lista (para mostrar el ahorro)
ALTER TABLE precio_cliente ADD COLUMN actualizado_en timestamptz NOT NULL DEFAULT now();  -- caché con TTL de GetMultiPrecioExpo
ALTER TABLE proveedor ADD COLUMN color_hex text;
ALTER TABLE anuncio   ADD COLUMN titulo text;
ALTER TABLE promocion ADD COLUMN titulo text;
ALTER TABLE pedido    ADD COLUMN folio bigint GENERATED ALWAYS AS IDENTITY;
ALTER TABLE pedido    ADD COLUMN promociones jsonb NOT NULL DEFAULT '[]';    -- premios ganados al colocar (texto ya formateado)
CREATE INDEX ON pedido (id_persona, creado_en DESC);

-- Snapshot de Indigo para el formulario del pedido
CREATE TABLE direccion (
  id         integer PRIMARY KEY,                -- vDirecciones.IdDireccion
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  etiqueta   text NOT NULL, texto text NOT NULL
);
CREATE INDEX ON direccion (id_persona);
CREATE TABLE transporte (id integer PRIMARY KEY, nombre text NOT NULL);
CREATE TABLE uso_cfdi (id smallint PRIMARY KEY, clave text NOT NULL UNIQUE, descripcion text NOT NULL);  -- mSatUsoCFDI

-- Sesiones de la tienda: el token opaco viaja en `Authorization: Bearer`; aquí solo se guarda su SHA-256.
CREATE TABLE sesion (
  token_hash bytea PRIMARY KEY,
  usuario_id bigint NOT NULL REFERENCES usuario(id) ON DELETE CASCADE,
  creado_en  timestamptz NOT NULL DEFAULT now(),
  expira_en  timestamptz NOT NULL
);
CREATE INDEX ON sesion (usuario_id);
