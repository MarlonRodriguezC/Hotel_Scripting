# Hotel Primera Entrega / MVP

Sistema mínimo de gestión de hotel que permite manejar **habitaciones, clientes y reservas** por **consola**.  
La demo muestra: **listar → verificar disponibilidad → crear reserva → rechazar solapes**.

---

## 1) Alcance del MVP
- **Datos en memoria**: habitaciones, clientes y reservas.
- **Crear y consultar reservas** con validación (check-in / check-out).
- **Cálculo básico** del total por noches (precio base por noche).
- **Ejecución por consola** del flujo completo.

**1.1 Funcionalidades del MVP (primera entrega)**

- **Datos en memoria:** habitaciones, clientes y reservas.
- **Crear y consultar** reservas con validación de solapes (check-in / check-out).
- **Cálculo básico** del total por noches (precio base por noche).
- **Ejecución por consola** del flujo completo.

**1.2 Fuera del MVP (próximas iteraciones)**

- **Roles y permisos** (recepción, limpieza, gerencia).
- **Cola de limpieza** vinculada a estados de habitación.
- **Facturación simulada** (IVA configurable, no válida para efectos fiscales).
- **Persistencia a archivos** (JSON) o base de datos real.

> Lo que queda **fuera** de esta entrega (para siguientes iteraciones): roles y permisos, cola de limpieza, facturación simulada, persistencia a archivo o BD, reportes.

---

## 2) Arquitectura mínima

- **Modelos (POO)**: `Room`, `Customer`, `Reservation`.
- **Servicio de dominio**: `ReservationService`
  - `IsRoomAvailable(roomId, from, to)`
  - `CreateReservation(roomId, customerId, checkIn, checkOut)`
  - `GetAllReservations()`

> El diseño queda listo para escalar luego a patrones `Singleton/Factory/Strategy` sin romper el MVP.

---

## 3) Estructuras de datos
- **Listas y diccionarios** en memoria para entidades.
- **Sin árboles**.

---

## 4) Diagrama de clases (texto)

- **Room**  
  Campos: `Id`, `Number`, `Capacity`, `PricePerNight`, `Status`

- **Customer**  
  Campos: `Id`, `FullName`, `Email`

- **Reservation**  
  Campos: `Id`, `RoomId`, `CustomerId`, `CheckIn`, `CheckOut`, `Total`

- **ReservationService**  
  - `IsRoomAvailable(roomId, from, to) : bool`  
  - `CreateReservation(roomId, customerId, checkIn, checkOut) : Reservation?`  
  - `GetAllReservations() : IEnumerable<Reservation>`

---

## 5) Estructura de proyecto sugerida

```text
HotelManager/
├─ Program.cs
├─ Models/
│  ├─ Room.cs
│  ├─ Customer.cs
│  └─ Reservation.cs
└─ Services/
   └─ ReservationService.cs
```

---

## 6) Cómo ejecutar

1. Asegúrate de tener **.NET 8 SDK** instalado.

**Qué verás (ejemplo):**
```
== Habitaciones ==
Room 101 (cap 2) - Available - $150000.00/night
Room 102 (cap 3) - Available - $220000.00/night

¿Room 101 disponible del 2025-09-01 al 2025-09-04? Sí
Reserva creada: Res#1 - Room:1 Customer:1 2025-09-01→2025-09-04 Total $450000.00

Intentando reserva solapada del 2025-09-03 al 2025-09-05...
Correcto: detectado solape y no se reservó.

== Reservas actuales ==
Res#1 - Room:1 Customer:1 2025-09-01→2025-09-04 Total $450000.00

Listo. (MVP de estructura funcionando)
```

---

## 7) Miembros del equipo
- **Nombre:** Marlon Daniel Rodriguez Caro, Santiago Velasco Mosquera, Juan
Sebastian Gonzalez Arcila y Luis Fernando Parra Castilla
