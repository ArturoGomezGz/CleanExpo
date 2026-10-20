-- Datos de desarrollo (ficticios). No usar en producción.
-- Cuenta demo: demo@expo.test / expo (distribuidor). Con MR_MODE=fake el precio es precio_lista × 0.88 para distribuidores.
INSERT INTO sucursal (id, nombre, indigo_db) VALUES (1,'GDL','Indigo'),(2,'QRO','QtroIndigo'),(3,'MEX','EdomxIndigo');
INSERT INTO proveedor (id_persona, nombre, descripcion, color_hex) VALUES
  (9001,'Laboratorios Aurora','Analgésicos y antiinflamatorios de uso diario.','#0b6e4f'),
  (9002,'Farma Norte','Antibióticos y genéricos intercambiables.','#1d4ed8');
INSERT INTO anuncio (proveedor_id, titulo, texto, orden) VALUES
  (9001,'Compra 10, lleva 1','En toda la línea de analgésicos.',1),
  (9002,'Genéricos con descuento','Descuento escalonado por volumen.',1);
INSERT INTO laboratorio (id_laboratorio, nombre, proveedor_id) VALUES (100,'Laboratorio Demo',9001),(101,'Laboratorio Norte',9002);
INSERT INTO producto (id_producto, id_laboratorio, nombre, presentacion, codigo, iva, costo, precio_lista, status_indigo) VALUES
  (1,100,'Paracetamol 500 mg','Caja 20 tabs','MR00001',false,10,14.20,0),
  (2,100,'Ibuprofeno 400 mg','Caja 10 tabs','MR00002',false,15,20.45,0),
  (3,101,'Amoxicilina 500 mg','Caja 12 cápsulas','MR00003',false,64,86.40,3),
  (4,101,'Protector solar FPS 50','Frasco 120 ml','MR00004',true,135,182.25,4);
INSERT INTO cliente (id_persona, nombre, sucursal_id, tipo_cliente, credito_expo, id_direccion_fiscal, id_direccion_entrega)
VALUES (1001,'Farmacia Demo',1,2,120000,1,1);
INSERT INTO direccion (id, id_persona, etiqueta, texto) VALUES
  (1,1001,'Matriz','Av. Juárez 120, Guadalajara, Jal.'),
  (2,1001,'Sucursal Centro','Calle Hidalgo 45, Zapopan, Jal.');
INSERT INTO transporte (id, nombre) VALUES (1,'Entrega MR'),(2,'Paquetería (por cobrar)'),(3,'Recoge en sucursal');
INSERT INTO uso_cfdi (id, clave, descripcion) VALUES (1,'G01','Adquisición de mercancías'),(2,'G03','Gastos en general'),(3,'S01','Sin efectos fiscales');
INSERT INTO usuario (id_email, id_persona, email, password_hash, tipo_usuario, sucursal_id)
VALUES (501,1001,'demo@expo.test','$argon2id$v=19$m=19456,t=2,p=1$tSZ39jmqWdOxZwERvEt7yg$fOs9O3EWQPkMnUE57SO/EmUEkw3AtOmfhcoLCd7JO6E',1,1);
INSERT INTO precio_cliente (id_persona, id_producto, precio) VALUES (1001,1,12.50),(1001,2,18.00);
INSERT INTO setting (clave, valor) VALUES ('expo.monedas','on'),('expo.promociones','on'),('expo.ruleta','on');
INSERT INTO articulo (proveedor_id, nombre, descripcion, cantidad, orden) VALUES (9001,'caja de paracetamol 500 mg','Obsequio',100,1);
INSERT INTO promocion_tipo (id, nombre) VALUES (2,'acumulable'),(3,'no acumulable');
INSERT INTO promocion (proveedor_id, id_tipo_promocion, piezas, activa, titulo) VALUES (9001,2,10,true,'Compra 10, lleva 1');
INSERT INTO promocion_articulo (promocion_id, articulo_id, cantidad) SELECT currval('promocion_id_seq'), id, 1 FROM articulo LIMIT 1;
