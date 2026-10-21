# Laboratorio 06 Waypoints y circuitos

## Informe de desarrollo y verificacion

**Proyecto:** LAB06 Waypoints
**Editor objetivo:** Unity 6 `6000.3.17f1`
**Plataforma:** Windows 64-bit
**Escenas:** `Assets/Scenes/TankRacing.unity` y `Assets/Scenes/CarCircuit.unity`

**Estudiante:** ____________________________________
**Curso / grupo:** _________________________________
**Docente:** _______________________________________
**Fecha de entrega:** ______________________________

## Resumen

Este informe documenta la implementacion del Laboratorio 06 sobre navegacion mediante waypoints y seguimiento de circuitos. El proyecto Unity contiene la escena de tanques del starter con nueve puntos ordenados y cuatro agentes configurados; tambien incorpora un circuito costero de carreras con quince waypoints, cuatro autos, superficie y barreras con colision, conteo de vueltas, seguimiento directo y seguimiento de un tracker movil. El escenario de autos se genera en el modo de edicion al abrir la escena, para que su geometria, jerarquia, puntos y componentes puedan inspeccionarse sin entrar en Play. Ambas escenas incluyen un panel para cambiar de escena, enfocar vehiculos y controlar el modo de movimiento.

Los scripts compilaron con las bibliotecas del Editor objetivo y se revisaron las referencias serializadas de la escena de tanques. No se pudo completar una prueba visual de Play en esta sesion porque Unity Licensing rechazo la conexion de License Client. Por tanto, se describe por separado la implementacion comprobada estaticamente y la prueba interactiva que queda por registrar en las capturas.

## 1. Objetivos

1. Comprender como una secuencia de posiciones de referencia puede dirigir la navegacion de un agente.
2. Configurar un vehiculo para seguir una ruta ordenada y cerrada.
3. Comparar el seguimiento directo de waypoints con el seguimiento de un objeto tracker.
4. Modificar el escenario y sus recursos para representar una carrera de autos.
5. Inspeccionar la estructura, los componentes y las referencias desde el Editor de Unity.

## 2. Recursos y entorno

El proyecto conserva los recursos del starter suministrado: escena TankRacing, modelos FBX del tanque y la palmera, texturas de arena, materiales de equipo y los datos de iluminacion/reflexion. Los nuevos elementos del circuito se construyen con primitivas y mallas generadas por el proyecto. No dependen de modelos externos que no esten en el paquete de recursos.

El Editor objetivo es Unity 6 `6000.3.17f1`. La escena de tanques preserva el contenido base y agrega la ruta y sus seguidores. La escena `CarCircuit` incluye un constructor de vista previa en modo edicion (`ExecuteAlways`): al abrirla, la jerarquia se completa con la pista, los waypoints, los autos, las barreras, la meta y la interfaz. El Editor guarda esos objetos en la escena al completar la vista previa. La ejecucion de ese paso requiere que Unity importe los scripts y el paquete Input System; en esta sesion no se pudo abrir el Editor para confirmar la vista final por el rechazo de Unity Licensing.

## 3. Procedimiento y actividades del laboratorio

| Paso del laboratorio | Desarrollo realizado |
|---|---|
| 1. Crear un proyecto 3D en Unity | Se configuro el proyecto para Unity 6 `6000.3.17f1`, con ambas escenas registradas en Build Settings. |
| 2. Obtener los recursos del tutorial | Se importaron y organizaron los recursos base del starter proporcionado. El contenido del tutorial se uso como referencia para reproducir la tecnica de circuito y tracker. |
| 3. Importar los recursos al proyecto | Los FBX, texturas, materiales, escena y datos de iluminacion del starter permanecen dentro de `Assets`. |
| 4. Abrir TankRacing | `Assets/Scenes/TankRacing.unity` conserva el escenario original que se usa en la primera parte del laboratorio. |
| 5. Inspeccionar el escenario | Se identificaron los tanques, el terreno, las palmeras, la camara y la iluminacion antes de ampliar la ruta. |
| 6. Duplicar y ordenar las palmeras | Se distribuyeron nueve palmeras de referencia alrededor de una ruta cerrada. Hay nueve objetos `Palm Waypoint 01` a `Palm Waypoint 09` en el circuito. |
| 7. Revisar `FollowWP` | El componente conserva una lista serializada, velocidad, giro, radio de llegada, punto inicial y separacion de carril. |
| 8. Asignar el script al tanque | Los cuatro tanques tienen `FollowWP` en la escena. |
| 9. Bloquear y desbloquear el Inspector | La lista de puntos esta serializada en cada componente, por lo que se conserva al guardar la escena. |
| 10. Asignar los puntos de la ruta | Cada seguidor referencia los nueve transforms en el mismo orden; no hace falta volver a arrastrarlos manualmente al abrir el proyecto. |
| 11. Desbloquear el Inspector | La configuracion de cada tanque puede revisarse por separado en el Inspector. |
| 12. Ejecutar y probar el movimiento | La logica de movimiento y colision esta implementada. La prueba Play en el Editor queda pendiente de evidencia visual por el bloqueo de licencia descrito en la seccion 9. |
| 13. Cambiar velocidades | Cada tanque tiene valores de velocidad y carril configurables. En los autos tambien se asignan velocidades distintas para poder comparar el movimiento. |
| 14. Repetir para otros tanques | Blue, Red, Yellow y Green siguen la misma ruta cerrada desde posiciones iniciales y carriles distintos. |
| 15. Ejecutar y verificar todos los agentes | Se compilaron los scripts y se validaron las referencias serializadas. La ejecucion simultanea en Play requiere la captura pendiente una vez Unity Licensing permita abrir el Editor. |

## 4. Solucion implementada

### 4.1 Ruta de tanques

La ruta de tanques utiliza nueve transforms asociados a palmeras colocadas a lo largo del circuito. `WaypointCircuit` mantiene el orden y permite inspeccionar el trazado en la vista Scene. `FollowWP` calcula la direccion hacia el punto activo, orienta el vehiculo respetando `turnSpeed`, avanza a la velocidad configurada y pasa al siguiente punto al entrar en `arrivalDistance`. Al llegar al ultimo indice, vuelve al primero.

Los cuatro tanques comparten la ruta, pero arrancan en indices diferentes y usan offsets laterales. Esto separa las trayectorias y permite comparar velocidades. Cuando un vehiculo tiene `Rigidbody`, el movimiento usa `MovePosition` y `MoveRotation` en `FixedUpdate`; los objetos sin cuerpo rigido conservan el movimiento por transform.

### 4.2 Circuito de autos visible en el Editor

La escena `CarCircuit` tiene quince waypoints de control que definen una curva Catmull-Rom cerrada. El componente `CarCircuitDemo` usa `ExecuteAlways` para crear la vista previa en modo de edicion y solicitar que el Editor la guarde en la escena. Esta creacion se ejecuta al abrir la escena con los scripts compilados; la vista final debe registrarse con la captura indicada cuando el Editor pueda abrirse.

El escenario representa un circuito costero tropical con agua, isla de arena, infield, franja verde de escape, asfalto, lineas de borde, pianos, palmeras con frondas, garages, graderia, iluminacion, camara, señal de salida/meta y cuatro autos de distintos colores. Los autos se construyen con primitivas; sus componentes de seguimiento, fisica y referencias a la ruta quedan visibles para inspeccion.

### 4.3 Fisica y colisiones

- La superficie asfaltada tiene una malla cerrada y un `MeshCollider` no convexo para la pista estatica.
- Las barreras de seguridad y los pianos tienen colliders de forma cubica.
- Cada auto tiene `Rigidbody` dinamico sin gravedad, altura y vuelco restringidos, un `BoxCollider` de chasis y cuatro `WheelCollider` con radio y suspension configurados. Las ruedas visibles son decorativas para evitar colisiones duplicadas.
- La linea de salida/meta incluye un `BoxCollider` configurado como trigger, un `Rigidbody` cinemático y un `RaceLapCounter` que cuenta una vuelta cuando el auto abandona y vuelve a cruzar la zona.
- Los elementos decorativos que no participan en el contacto fisico no generan colision innecesaria.

### 4.4 Seguimiento directo y tracker

En el modo directo, cada auto usa `FollowWP` para seguir la lista ordenada. En el modo tracker, `CircuitTracker` recorre la ruta suavizada y `FollowTracker` persigue la posicion movil manteniendo una distancia. El panel de navegacion permite alternar entre los dos metodos mientras la escena esta en Play. El tracker y sus componentes permanecen en la escena para que puedan inspeccionarse antes de ejecutar.

### 4.5 Panel de navegacion

Las dos escenas incluyen un panel UGUI configurado como Screen Space Overlay y visible en Game incluso cuando el Editor no esta en Play. Incluye botones para cambiar entre la escena de tanques y la de autos, avanzar o retroceder entre vehiculos, enfocar el vehiculo seleccionado, volver a la vista general y cambiar el modo de movimiento en la carrera. En la escena de autos tambien muestra vueltas completadas. La generacion visual y la interaccion de botones quedan pendientes de comprobar en el Editor.

El proyecto incorpora el paquete Unity Input System y configura el manejo de entrada nuevo para la interfaz. La advertencia deprecada del Input Manager mostrada en la captura se aborda con esa migracion; el paquete debe terminar de resolverse y el Editor puede pedir un reinicio para aplicar el cambio.

## 5. Ejercicios propuestos

### Ejercicio 1. Completar Following a Circuit (& Tracker)

Se implementan los dos enfoques descritos por el tutorial: seguidores que recorren waypoints directamente y seguidores que persiguen un tracker movil. La escena de autos hace observable la comparacion mediante el boton de modo del panel. El tracker se desplaza sobre una curva suavizada; cada auto conserva su controlador directo para que el cambio de modo no requiera reconfigurar la escena.

### Ejercicio 2. Cambiar el escenario y los recursos para una carrera de autos

Se agrego una escena independiente con una pista cerrada, marcaciones de borde, pianos, barreras, meta, garages, graderia, palmeras y cuatro autos. La pista y los obstaculos relevantes tienen colisionadores. Los vehiculos tienen fisica y siguen el circuito. El escenario puede inspeccionarse en el Editor sin Play y navegarse con el panel cuando se ejecuta.

### Ejercicio 3. Elaborar el informe

Este documento resume las actividades, la logica, los ejercicios, el cuestionario, las comprobaciones realizadas y las capturas que deben incorporarse como evidencia visual para la entrega.

## 6. Cuestionario

### 1. ¿Que son los waypoints?

Son posiciones de referencia situadas en el espacio del juego. Ordenadas en una secuencia, indican por donde debe pasar un agente. El agente se dirige al punto activo y cambia al siguiente al alcanzar la distancia definida. Si el indice final vuelve al primero, la ruta es cerrada y puede recorrerse repetidamente.

### 2. ¿Como se implementan en Unity para formar un circuito?

Se crean objetos con `Transform` en las posiciones del recorrido, se guardan en orden en un componente y se asigna esa lista al vehiculo. En cada paso de actualizacion, un script calcula la direccion al punto actual, rota el vehiculo hacia el objetivo, lo mueve a una velocidad configurada y comprueba si llego. Al llegar, avanza el indice; despues del ultimo punto, reinicia la secuencia. En este proyecto, `FollowWP` ejecuta ese comportamiento. `WaypointCircuit` organiza la ruta, y `CircuitTracker` ofrece la variante de seguimiento indirecto.

## 7. Verificacion realizada y pendiente

**Comprobaciones realizadas:**

- Compilacion de todos los scripts runtime y de los bloques de Editor con las bibliotecas de Unity `6000.3.17f1`.
- Revisión de unicidad de IDs de documentos YAML y de las nueve referencias de waypoint en los cuatro seguidores de tanque.
- Revision de los nombres y referencias de componentes de las escenas, de los identificadores de scripts y de la configuracion de Build Settings.
- Revision estructural del circuito de autos: punto de entrada en modo edicion, malla de pista con collider, barreras, cuerpo rigido/collider de autos, trigger de meta y panel UGUI.

**Prueba pendiente:** Unity Licensing rechazo la conexion a `LicenseClient` al iniciar el Editor en esta sesion. Por eso no se pudo abrir Play y comprobar visualmente el movimiento, las colisiones, el cambio de escenas, el enfoque de camara y el contador de vueltas. No se presentan esas acciones como probadas. Al completar esa ejecucion, agrega las capturas correspondientes en los espacios de la seccion 8.

## 8. Registro fotografico

Inserta una captura de pantalla en cada espacio. Las primeras capturas deben mostrar la escena en modo de edicion para evidenciar que la jerarquia y el circuito se inspeccionan sin Play. Las capturas de funcionamiento se toman despues de ejecutar la prueba en Unity.

[[CAPTURE: Captura 1. Unity Hub y version del proyecto|Muestra el proyecto abierto desde la carpeta de trabajo y la version 6000.3.17f1.]]

[[CAPTURE: Captura 2. Escena TankRacing en modo de edicion|Muestra la ruta de nueve palmeras/waypoints, los cuatro tanques y la jerarquia.]]

[[CAPTURE: Captura 3. Inspector de FollowWP|Muestra un tanque seleccionado y sus nueve referencias, velocidad, giro, radio y carril.]]

[[CAPTURE: Captura 4. Escena CarCircuit sin Play|Muestra el circuito completo, los autos, puntos, decoracion, barreras y jerarquia generada.]]

[[CAPTURE: Captura 5. Inspector de fisica y colisiones|Muestra el MeshCollider de pista, un Rigidbody/BoxCollider de auto y el trigger de meta.]]

[[CAPTURE: Captura 6. Panel UGUI de navegacion|Muestra los controles para cambiar de escena, seleccionar vehiculo y alternar waypoints/tracker.]]

[[CAPTURE: Captura 7. Ejecucion Play y movimiento|Espacio reservado para evidencia de la ruta recorriendose cuando la licencia permita ejecutar Unity.]]

[[CAPTURE: Captura 8. Colision y conteo de vueltas|Espacio reservado para evidencia del contacto con barreras y del contador al cruzar la meta.]]

## 9. Conclusiones

El proyecto integra el seguimiento por waypoints en el escenario original y extiende la tecnica a una carrera de autos. La ruta de tanques permite modificar sus referencias y parametros desde el Inspector. La escena de autos hace visible el circuito y su jerarquia en modo de edicion, incluye colisiones en la pista y vehiculos, y ofrece dos metodos de seguimiento desde el panel. La interfaz permite pasar de una escena a otra y navegar por los vehiculos.

La evidencia tecnica disponible confirma que los scripts compilan en las variantes runtime y Editor, y que `TankRacing` contiene las referencias serializadas previstas. `CarCircuit` contiene el componente constructor y su vista previa se produce al abrirla en el Editor. La comprobacion visual e interactiva en Play queda condicionada a que Unity Licensing permita abrir el Editor. Las capturas reservadas permiten completar esa parte del informe sin atribuirse resultados que no se pudieron observar.

## 10. Referencias

- Unity Learn. [Waypoints](https://learn.unity.com/tutorial/waypoints?uv=2019.4&courseId=5dd851beedbc2a1bf7b72bed&projectId=5e0b6dd4edbc2a00200e3641).
- Unity Learn. [Following a Circuit & Tracker](https://learn.unity.com/tutorial/following-a-circuit).
- Unity Technologies. [Input System package 1.17.0](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/index.html).
- Millington, I. y Funge, J. (2019). *Artificial Intelligence for Games* (3.ª ed.). CRC Press.
