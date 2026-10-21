# Laboratorio 06 Waypoints y circuitos

## Informe de desarrollo y verificacion

**Proyecto:** LAB06 Waypoints
**Editor objetivo:** Unity 6 `6000.3.17f1`
**Plataforma:** Windows 64-bit
**Escenas:** `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/TankRacing.unity` y `Assets/Scenes/CarCircuit.unity`

**Estudiante:** ____________________________________
**Curso / grupo:** _________________________________
**Docente:** _______________________________________
**Fecha de entrega:** ______________________________

## Resumen

Este informe documenta la implementacion del Laboratorio 06 sobre navegacion mediante waypoints y seguimiento de circuitos. El proyecto abre con un menu que permite elegir por separado la ruta de tanques o la carrera de autos. La escena de tanques contiene nueve puntos ordenados y cuatro agentes; la carrera costera incorpora quince waypoints, un auto controlable, tres rivales, colisiones, conteo de vueltas y modos de seguimiento directo o mediante tracker. El circuito y los objetos del ejercicio se pueden inspeccionar desde Unity sin iniciar Play; al ejecutar, el HUD presenta los controles y permite cambiar de metodo o volver al selector.

Los scripts runtime compilaron con las bibliotecas y paquetes de Unity 6 `6000.3.17f1`; se revisaron las referencias serializadas de la escena de tanques y la estructura del selector y HUD. La compilacion externa no sustituye una prueba interactiva de Play. El Editor de Unity inicio el cliente de licencias pero esta sesion no obtuvo una interfaz controlable ni una corrida completa del juego; por eso las comprobaciones de movimiento y clics se identifican honestamente como pendientes de evidencia.

## 1. Objetivos

1. Comprender como una secuencia de posiciones de referencia puede dirigir la navegacion de un agente.
2. Configurar un vehiculo para seguir una ruta ordenada y cerrada.
3. Comparar el seguimiento directo de waypoints con el seguimiento de un objeto tracker.
4. Modificar el escenario y sus recursos para representar una carrera de autos.
5. Inspeccionar la estructura, los componentes y las referencias desde el Editor de Unity.

## 2. Recursos y entorno

El proyecto conserva los recursos del starter suministrado: escena TankRacing, modelos FBX del tanque y la palmera, texturas de arena, materiales de equipo y los datos de iluminacion/reflexion. Los nuevos elementos del circuito se construyen con primitivas y mallas generadas por el proyecto. No dependen de modelos externos que no esten en el paquete de recursos.

El Editor objetivo es Unity 6 `6000.3.17f1`. El Build Settings inicia en `MainMenu`, que contiene dos opciones grandes y separadas para `TankRacing` y `CarCircuit`. La escena de tanques conserva el contenido base y agrega la ruta y sus seguidores. La escena `CarCircuit` construye y guarda su vista previa en modo de edicion (`ExecuteAlways`), para mostrar la pista, waypoints, coches, barreras, meta e interfaz sin Play. El paquete Input System del proyecto proporciona las entradas de teclado y el modulo de interfaz.

## 3. Procedimiento y actividades del laboratorio

| Paso del laboratorio | Desarrollo realizado |
|---|---|
| 1. Crear un proyecto 3D en Unity | Se configuro el proyecto para Unity 6 `6000.3.17f1`; el selector `MainMenu` aparece primero en Build Settings, seguido de ambas escenas del laboratorio. |
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
| 12. Ejecutar y probar el movimiento | `FollowWP` controla las rutas cerradas. La carrera asigna el auto Azure al jugador y muestra los controles WASD/flechas y freno de mano en el HUD. La compilacion runtime se completo; la evidencia interactiva de Play queda pendiente. |
| 13. Cambiar velocidades | Cada tanque tiene valores de velocidad y carril configurables. En los autos tambien se asignan velocidades distintas para poder comparar el movimiento. |
| 14. Repetir para otros tanques | Blue, Red, Yellow y Green siguen la misma ruta cerrada desde posiciones iniciales y carriles distintos. |
| 15. Ejecutar y verificar todos los agentes | Se compilaron los scripts y se validaron las referencias serializadas. La ejecucion simultanea en Play requiere la captura pendiente una vez Unity Licensing permita abrir el Editor. |

## 4. Solucion implementada

### 4.1 Ruta de tanques

La ruta de tanques utiliza nueve transforms asociados a palmeras colocadas a lo largo del circuito. `WaypointCircuit` mantiene el orden y permite inspeccionar el trazado en la vista Scene. `FollowWP` calcula la direccion hacia el punto activo, orienta el vehiculo respetando `turnSpeed`, avanza a la velocidad configurada y pasa al siguiente punto al entrar en `arrivalDistance`. Al llegar al ultimo indice, vuelve al primero.

Los cuatro tanques comparten la ruta, pero arrancan en indices diferentes y usan offsets laterales. Esto separa las trayectorias y permite comparar velocidades. Cuando un vehiculo tiene `Rigidbody`, el movimiento usa `MovePosition` y `MoveRotation` en `FixedUpdate`; los objetos sin cuerpo rigido conservan el movimiento por transform.

### 4.2 Circuito de autos visible en el Editor

La escena `CarCircuit` tiene quince waypoints de control que definen una curva Catmull-Rom cerrada. El componente `CarCircuitDemo` usa `ExecuteAlways` para construir y guardar el circuito en modo de edicion. El selector de escenas abre esta carrera; los controles se habilitan al ejecutar.

El escenario representa un circuito costero tropical con agua, isla de arena, infield, franja verde de escape, asfalto, lineas de borde, pianos, palmeras con frondas, garages, graderia, iluminacion, camara, señal de salida/meta y cuatro autos de distintos colores. El auto Azure pertenece al jugador; los otros tres son rivales de waypoint distribuidos en carriles separados. La conduccion utiliza aceleracion, direccion, frenado, velocidad maxima y friccion lateral, y actualiza la orientacion de las ruedas visibles con sus `WheelCollider`.

### 4.3 Fisica y colisiones

- La superficie asfaltada tiene una malla cerrada y un `MeshCollider` no convexo para la pista estatica.
- Las barreras de seguridad y los pianos tienen colliders de forma cubica.
- Cada auto tiene `Rigidbody` dinamico sin gravedad, altura y vuelco restringidos, un `BoxCollider` de chasis y cuatro `WheelCollider` con radio y suspension configurados. En el auto del jugador, las ruedas delanteras giran y las traseras reciben par; sus modelos visibles se sincronizan con la suspension.
- La linea de salida/meta incluye un `BoxCollider` configurado como trigger, un `Rigidbody` cinemático y un `RaceLapCounter` que cuenta una vuelta cuando el auto abandona y vuelve a cruzar la zona.
- Los elementos decorativos que no participan en el contacto fisico no generan colision innecesaria.

### 4.4 Seguimiento directo y tracker

En el modo directo, los rivales usan `FollowWP` para seguir la lista ordenada. En el modo tracker, `CircuitTracker` recorre la ruta suavizada y `FollowTracker` persigue la posicion movil. El boton de modo del HUD permite comparar los metodos durante Play; el auto Azure sigue bajo control del jugador en ambos casos. Los rivales frenan ante curvas cerradas para reducir los recortes sobre otros carriles.

### 4.5 Panel de navegacion

El selector dedicado muestra dos tarjetas: `Modo tanques` y `Carrera de autos`. Cada circuito contiene un HUD compacto con controles, velocidad durante la carrera, selector de modo waypoint/tracker, camara de seguimiento o vista general, botones de cambio de escena y retorno al selector. Los botones se enlazan al inicio de Play, se crea el EventSystem con `InputSystemUIInputModule` y la interfaz utiliza la fuente `LegacyRuntime.ttf` compatible con Unity 6. Esta ultima corrige la excepcion que detenia la generacion de textos y dejaba vacia la mayor parte del panel.

El proyecto incorpora el paquete Unity Input System y configura el manejo de entrada nuevo para la interfaz. La advertencia deprecada del Input Manager mostrada en la captura se aborda con esa migracion; el paquete debe terminar de resolverse y el Editor puede pedir un reinicio para aplicar el cambio.

## 5. Ejercicios propuestos

### Ejercicio 1. Completar Following a Circuit (& Tracker)

Se implementan los dos enfoques descritos por el tutorial: los rivales recorren waypoints directamente o persiguen el tracker movil sobre una curva suavizada. El HUD cambia entre ellos durante Play. El usuario conduce Azure con WASD o las flechas; la barra espaciadora activa el freno de mano.

### Ejercicio 2. Cambiar el escenario y los recursos para una carrera de autos

Se agrego una escena independiente con una pista cerrada, marcaciones de borde, pianos, barreras, meta, garages, graderia, palmeras y cuatro autos. La pista y los obstaculos relevantes tienen colisionadores. El jugador conduce un auto y los tres rivales siguen carriles de waypoint con desaceleracion en curva. La escena puede inspeccionarse sin Play y se alcanza desde la tarjeta `Carrera de autos`.

### Ejercicio 3. Elaborar el informe

Este documento resume las actividades, la logica, los ejercicios, el cuestionario, las comprobaciones realizadas y las capturas que deben incorporarse como evidencia visual para la entrega.

## 6. Cuestionario

### 1. ¿Que son los waypoints?

Son posiciones de referencia situadas en el espacio del juego. Ordenadas en una secuencia, indican por donde debe pasar un agente. El agente se dirige al punto activo y cambia al siguiente al alcanzar la distancia definida. Si el indice final vuelve al primero, la ruta es cerrada y puede recorrerse repetidamente.

### 2. ¿Como se implementan en Unity para formar un circuito?

Se crean objetos con `Transform` en las posiciones del recorrido, se guardan en orden en un componente y se asigna esa lista al vehiculo. En cada paso de actualizacion, un script calcula la direccion al punto actual, rota el vehiculo hacia el objetivo, lo mueve a una velocidad configurada y comprueba si llego. Al llegar, avanza el indice; despues del ultimo punto, reinicia la secuencia. En este proyecto, `FollowWP` ejecuta ese comportamiento. `WaypointCircuit` organiza la ruta, y `CircuitTracker` ofrece la variante de seguimiento indirecto.

## 7. Verificacion realizada y pendiente

**Comprobaciones realizadas:**

- Compilacion externa de todos los scripts runtime con referencias de Unity `6000.3.17f1`, incluida la UI, el Input System y VehiclesModule.
- Revisión de unicidad de IDs de documentos YAML y de las nueve referencias de waypoint en los cuatro seguidores de tanque.
- Revision de los nombres y referencias de componentes de las escenas, de los identificadores de scripts y de la configuracion de Build Settings.
- Revision estructural del circuito de autos: punto de entrada en modo edicion, malla con collider, barreras, Rigidbody/BoxCollider de vehiculos, trigger de meta, WheelColliders, selector y HUD.

**Prueba pendiente:** aunque Unity resolvio entitlement Unity Personal en un log previo, el proceso iniciado para esta verificacion no llego a ofrecer una ventana controlable. Por eso no pude confirmar en vivo los clics, el desplazamiento de los autos, colisiones ni las vueltas. No presento esa ejecucion como probada. Abre `MainMenu` o pulsa Play y agrega evidencia real en los espacios de la seccion 8.

## 8. Registro fotografico

Inserta una captura de pantalla en cada espacio. Las primeras capturas deben mostrar la escena en modo de edicion para evidenciar que la jerarquia y el circuito se inspeccionan sin Play. Las capturas de funcionamiento se toman despues de ejecutar la prueba en Unity.

[[CAPTURE: Captura 1. Unity Hub y version del proyecto|Muestra el proyecto abierto desde la carpeta de trabajo y la version 6000.3.17f1.]]

[[CAPTURE: Captura 2. Escena TankRacing en modo de edicion|Muestra la ruta de nueve palmeras/waypoints, los cuatro tanques y la jerarquia.]]

[[CAPTURE: Captura 3. Inspector de FollowWP|Muestra un tanque seleccionado y sus nueve referencias, velocidad, giro, radio y carril.]]

[[CAPTURE: Captura 4. Escena CarCircuit sin Play|Muestra el circuito completo, los autos, puntos, decoracion, barreras y jerarquia generada.]]

[[CAPTURE: Captura 5. Inspector de fisica y colisiones|Muestra el MeshCollider de pista, un Rigidbody/BoxCollider de auto y el trigger de meta.]]

[[CAPTURE: Captura 6. Menu de seleccion de modo|Muestra las tarjetas independientes para entrar al circuito de tanques o a la carrera de autos.]]

[[CAPTURE: Captura 7. Auto controlado en Play|Muestra el auto Azure, el HUD con WASD/flechas y una velocidad distinta de cero.]]

[[CAPTURE: Captura 8. Colision y conteo de vueltas|Espacio reservado para evidencia del contacto con barreras y del contador al cruzar la meta.]]

## 9. Conclusiones

El proyecto integra el seguimiento por waypoints en el escenario original y extiende la tecnica a una carrera conducible. La entrada separa el circuito de tanques del de autos; en la carrera, el jugador conduce y los rivales se distribuyen en carriles, reducen velocidad en curvas y comparan los metodos waypoint y tracker desde el HUD. La pista contiene colisiones y contador de vueltas; las escenas muestran su jerarquia y la interfaz en el Editor.

La evidencia tecnica confirma que los scripts runtime compilan con las referencias de Unity 6 y que las escenas, rutas, colliders, controles y modos estan conectados en el proyecto. El intento de abrir una ventana del Editor no proporciono una superficie de interfaz controlable en esta sesion, por lo que los resultados de Play permanecen pendientes hasta que se tomen las capturas de movimiento, seleccion y vueltas. El informe no atribuye esas pruebas a una corrida que no se pudo observar.

## 10. Referencias

- Unity Learn. [Waypoints](https://learn.unity.com/tutorial/waypoints?uv=2019.4&courseId=5dd851beedbc2a1bf7b72bed&projectId=5e0b6dd4edbc2a00200e3641).
- Unity Learn. [Following a Circuit & Tracker](https://learn.unity.com/tutorial/following-a-circuit).
- Unity Technologies. [Input System package 1.17.0](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.17/manual/index.html).
- Millington, I. y Funge, J. (2019). *Artificial Intelligence for Games* (3.ª ed.). CRC Press.
