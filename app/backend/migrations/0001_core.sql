-- Clean Expo — modelo núcleo en Postgres (borrador v0.1)
-- Origen: ExpoMR 7.0.0 (tablas mClientes, mUsuarios, mProductos, m/dCotizaciones, m/dPedidos, ...).
-- Monedas, promociones y ruleta NO están aquí: módulos opcionales pendientes de confirmar con MR.
-- Los ids de Indigo (id_persona, id_email, id_producto, id_laboratorio) se conservan como referencias externas.

CREATE TABLE sucursal (
  id            smallint PRIMARY KEY,           -- 1 GDL, 2 QRO, 3 MEX
  nombre        text NOT NULL,
  indigo_db     text NOT NULL                   -- Indigo | QtroIndigo | EdomxIndigo (se usa en la API interna)
);

CREATE TABLE setting (                           -- reemplaza mSysVar / GetSysVar
  clave text PRIMARY KEY, valor text NOT NULL
);

CREATE TABLE expo_dia (                          -- reemplaza mCalendario / ValidaDiaExpo
  fecha date PRIMARY KEY, hora_inicio smallint NOT NULL, hora_fin smallint NOT NULL
);

-- ---- Autenticación (reemplaza ASP.NET Identity; usuarios de la Expo) ----
CREATE TABLE usuario (
  id               bigserial PRIMARY KEY,
  id_email         integer NOT NULL UNIQUE,      -- Indigo.mEmails.IdEmail
  id_persona       integer NOT NULL,             -- Indigo.mPerEmp.IdPersona
  email            text NOT NULL UNIQUE,
  password_hash    text,
  tipo_usuario     smallint NOT NULL,            -- mTipoUsuario (1 = cliente)
  sucursal_id      smallint NOT NULL REFERENCES sucursal(id),
  ultima_notificacion timestamptz,
  creado_en        timestamptz NOT NULL DEFAULT now()
);

-- ---- Snapshot de Indigo ----
CREATE TABLE cliente (                           -- mClientes
  id_persona     integer PRIMARY KEY,
  nombre         text NOT NULL,
  id_documento   smallint, id_uso_cfdi smallint, id_tipo_venta smallint,
  id_direccion_fiscal integer, id_direccion_entrega integer,
  id_transporte  integer, id_porte smallint,
  sucursal_id    smallint NOT NULL REFERENCES sucursal(id),
  credito_expo   numeric(12,2) NOT NULL DEFAULT 0,   -- mClientes.Credito (ExpoMR)
  sincronizado_en timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE laboratorio (
  id_laboratorio integer PRIMARY KEY, nombre text NOT NULL,
  proveedor_id   integer                           -- stand dueño
);

CREATE TABLE producto (                          -- mProductos + vProductos
  id_producto    smallint PRIMARY KEY,
  id_laboratorio integer NOT NULL REFERENCES laboratorio(id_laboratorio),
  nombre text NOT NULL, presentacion text, formula text, codigo text,
  iva            boolean NOT NULL,
  descuento_pct  smallint NOT NULL DEFAULT 0,       -- descuento general
  costo          numeric(12,2),
  rank           smallint, especial boolean NOT NULL DEFAULT false,
  status_indigo  smallint NOT NULL,                 -- 0 activo, 3 nuevo, 4 liquidación, 5 pre-nuevo
  habilitar      boolean NOT NULL DEFAULT true
);

CREATE TABLE precio_cliente (                    -- resultado de GetMultiPrecioExpo por cliente
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  id_producto smallint NOT NULL REFERENCES producto(id_producto),
  precio numeric(12,2) NOT NULL,
  PRIMARY KEY (id_persona, id_producto)
);

CREATE TABLE descuento_cliente (                 -- dProductosDescuento
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  id_producto smallint NOT NULL REFERENCES producto(id_producto),
  descuento_pct numeric(5,2) NOT NULL,
  PRIMARY KEY (id_persona, id_producto)
);

-- ---- Carrito (mCotizaciones / dCotizaciones) ----
CREATE TABLE carrito (
  id bigserial PRIMARY KEY,
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  usuario_id bigint NOT NULL REFERENCES usuario(id),
  id_documento smallint, id_uso_cfdi smallint, id_tipo_venta smallint,
  id_transporte integer, id_porte smallint,
  id_direccion_fiscal integer, id_direccion_entrega integer,
  observaciones text,
  UNIQUE (id_persona, usuario_id)
);
CREATE TABLE carrito_item (
  carrito_id bigint NOT NULL REFERENCES carrito(id) ON DELETE CASCADE,
  id_producto smallint NOT NULL REFERENCES producto(id_producto),
  unitario numeric(12,4) NOT NULL, descuento numeric(9,2) NOT NULL DEFAULT 0,
  cantidad integer NOT NULL CHECK (cantidad > 0),
  confirmado boolean NOT NULL DEFAULT true,      -- PorConfirmar
  actualizado_en timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY (carrito_id, id_producto)
);

-- ---- Pedidos + outbox (mPedidos / dPedidos) ----
CREATE TYPE pedido_estado AS ENUM ('recibido','enviando','sincronizado','parcial','surtido','error');
CREATE TABLE pedido (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),   -- idempotency key hacia la API interna
  id_pedido_mr integer UNIQUE,                     -- ExpoMR.mPedidos.IdPedido cuando se confirma
  id_persona integer NOT NULL REFERENCES cliente(id_persona),
  usuario_id bigint NOT NULL REFERENCES usuario(id),
  sucursal_id smallint NOT NULL REFERENCES sucursal(id),
  id_documento smallint, id_uso_cfdi smallint, id_tipo_venta smallint,
  id_transporte integer, id_porte smallint,
  id_direccion_fiscal integer, id_direccion_entrega integer,
  observaciones text,
  estado pedido_estado NOT NULL DEFAULT 'recibido',
  creado_en timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE pedido_item (
  pedido_id uuid NOT NULL REFERENCES pedido(id) ON DELETE CASCADE,
  id_producto smallint NOT NULL REFERENCES producto(id_producto),
  unitario numeric(12,4) NOT NULL, descuento numeric(9,2) NOT NULL,
  cantidad integer NOT NULL, surtido integer NOT NULL DEFAULT 0,
  colocado boolean NOT NULL DEFAULT false,
  PRIMARY KEY (pedido_id, id_producto)
);
CREATE TABLE outbox (
  id bigserial PRIMARY KEY,
  pedido_id uuid NOT NULL REFERENCES pedido(id),
  intentos integer NOT NULL DEFAULT 0,
  siguiente_intento timestamptz NOT NULL DEFAULT now(),
  ultimo_error text, enviado_en timestamptz
);

-- ---- Stands / contenido / imágenes ----
CREATE TABLE proveedor (id_persona integer PRIMARY KEY, nombre text NOT NULL, logo_imagen_id uuid, descripcion text, color smallint);
CREATE TABLE anuncio   (id bigserial PRIMARY KEY, proveedor_id integer NOT NULL REFERENCES proveedor(id_persona), imagen_id uuid, texto text, orden smallint);
CREATE TABLE articulo  (id bigserial PRIMARY KEY, proveedor_id integer NOT NULL REFERENCES proveedor(id_persona), nombre text NOT NULL, descripcion text, cantidad integer, orden smallint);
CREATE TABLE imagen (                             -- metadatos; el binario va a object storage
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  clave_storage text NOT NULL, mime text NOT NULL, bytes integer NOT NULL,
  ancho integer, alto integer, variantes jsonb, creado_en timestamptz NOT NULL DEFAULT now()
);

-- ---- Bitácoras ----
CREATE TABLE log_visita (id bigserial PRIMARY KEY, usuario_id bigint, tipo text, referencia text, creado_en timestamptz NOT NULL DEFAULT now());
CREATE TABLE log_busqueda (id bigserial PRIMARY KEY, usuario_id bigint, termino text, creado_en timestamptz NOT NULL DEFAULT now());
