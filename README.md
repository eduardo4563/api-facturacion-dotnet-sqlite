# 🧾 API de Facturación Electrónica — .NET 8 + SQLite

Proyecto backend nivel portafolio profesional desarrollado con **ASP.NET Core .NET 8**, **SQLite**, **JWT**, **Swagger** y arquitectura en capas.

La idea de este proyecto es demostrar cómo construir una API empresarial para facturación sin depender de SQL Server, para que cualquier persona pueda clonarlo y ejecutarlo rápido.

---

## 🚀 Tecnologías usadas

- .NET 8
- ASP.NET Core Web API
- SQLite
- Entity Framework Core
- JWT Bearer Authentication
- Swagger / OpenAPI
- Arquitectura en capas

---

## 🧠 Arquitectura

```txt
Controllers
   ↓
Services
   ↓
Repositories
   ↓
DbContext
   ↓
SQLite
```

---

## 📁 Estructura

```txt
FacturacionAPI_Enterprise_SQLite/
├── Controllers/
├── Services/
├── Repositories/
├── Entities/
├── DTOs/
├── Data/
├── Middlewares/
├── Program.cs
├── appsettings.json
└── README.md
```

---

## ✅ Funcionalidades

### 🔐 Seguridad
- Login con JWT.
- Endpoints protegidos con `[Authorize]`.
- Swagger configurado para usar Bearer Token.

### 👤 Clientes
- Crear clientes.
- Listar clientes.

### 📦 Productos
- Crear productos.
- Listar productos.
- Control básico de stock.

### 🧾 Facturación
- Emitir factura.
- Calcular subtotal, IGV y total.
- Generar correlativo automático `F001-000001`.
- Descontar stock al emitir factura.
- Listar facturas con cliente y detalle.

---

## ▶️ Cómo ejecutar

### 1. Clonar el repositorio

```bash
git clone https://github.com/tuusuario/api-facturacion-dotnet-sqlite.git
cd api-facturacion-dotnet-sqlite
```

### 2. Ejecutar

```bash
dotnet run
```

### 3. Abrir Swagger

```txt
https://localhost:xxxx/swagger
```

> El archivo `facturacion.db` se crea automáticamente al ejecutar el proyecto.

---

## 🔐 Usuario demo

```json
{
  "username": "admin",
  "password": "123456"
}
```

---

## 🔥 Flujo de prueba en Swagger

### 1. Login

`POST /api/auth/login`

```json
{
  "username": "admin",
  "password": "123456"
}
```

Copia el token recibido.

---

### 2. Autorizar Swagger

Presiona **Authorize** y pega:

```txt
Bearer TU_TOKEN
```

---

### 3. Crear cliente

`POST /api/clientes`

```json
{
  "nombre": "Empresa Demo SAC",
  "documento": "20600000001",
  "email": "contacto@demo.com",
  "telefono": "999999999"
}
```

---

### 4. Crear producto

`POST /api/productos`

```json
{
  "codigo": "SERV-001",
  "nombre": "Servicio de desarrollo backend",
  "precio": 850,
  "stock": 10
}
```

---

### 5. Emitir factura

`POST /api/facturas/emitir`

```json
{
  "clienteId": 1,
  "detalles": [
    {
      "productoId": 1,
      "cantidad": 1
    }
  ]
}
```

---

## 📊 Ejemplo de respuesta de factura

```json
{
  "id": 1,
  "serie": "F001",
  "numero": 1,
  "clienteId": 1,
  "subTotal": 720.34,
  "igv": 129.66,
  "total": 850,
  "detalles": [
    {
      "productoId": 1,
      "cantidad": 1,
      "precioUnitario": 850,
      "total": 850
    }
  ]
}
```

---

## 🎯 Qué demuestra este proyecto

Este proyecto demuestra conocimientos en:

- Desarrollo backend con .NET.
- Diseño de APIs REST.
- Separación de responsabilidades.
- Uso de DTOs.
- Entity Framework Core.
- Autenticación JWT.
- Swagger profesional.
- Persistencia con SQLite.
- Lógica de negocio de facturación.

---

## 🧪 Estado del proyecto

✔ API ejecutable localmente  
✔ SQLite automático  
✔ Swagger documentado  
✔ JWT configurado  
✔ Flujo login → clientes → productos → facturación  

---

## 📌 Próximas mejoras

- Generación de PDF de factura.
- Exportación a Excel.
- Dashboard frontend en React.
- Docker.
- Tests unitarios.
- Refresh tokens.

---

## 👨‍💻 Autor

**Eduardo Jahir Montaño Condemayta**  
Backend / Full Stack Developer  
Lima, Perú

📧 eduardomontanocondemayta@gmail.com  
📱 WhatsApp: +51 941 797 953
