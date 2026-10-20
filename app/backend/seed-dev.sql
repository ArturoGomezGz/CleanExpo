-- Datos de desarrollo (ficticios). No usar en producción.
INSERT INTO sucursal (id, nombre, indigo_db) VALUES (1,'GDL','Indigo'),(2,'QRO','QtroIndigo'),(3,'MEX','EdomxIndigo');
INSERT INTO laboratorio (id_laboratorio, nombre) VALUES (100,'Laboratorio Demo');
INSERT INTO producto (id_producto, id_laboratorio, nombre, presentacion, iva, costo, status_indigo)
VALUES (1,100,'Paracetamol 500 mg','Caja 20 tabs',false,10,0),(2,100,'Ibuprofeno 400 mg','Caja 10 tabs',false,15,0);
INSERT INTO cliente (id_persona, nombre, sucursal_id) VALUES (1001,'Farmacia Demo',1);
INSERT INTO usuario (id_email, id_persona, email, tipo_usuario, sucursal_id) VALUES (501,1001,'demo@example.com',1,1);
INSERT INTO precio_cliente (id_persona, id_producto, precio) VALUES (1001,1,12.50),(1001,2,18.00);
INSERT INTO setting (clave, valor) VALUES ('expo.monedas','on'),('expo.promociones','on'),('expo.ruleta','on');
