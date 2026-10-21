# Laboratorio 06: Waypoints

## Objetivo y respuesta al cuestionario
Un waypoint es una posición de referencia en el mundo. Una secuencia ordenada de ellos define un recorrido que puede seguir un NPC para navegar por un mapa.

En Unity se colocan objetos Transform en las posiciones del circuito, se asignan en orden a un script y el vehículo avanza al punto actual, gira para orientarse hacia él y pasa al siguiente al llegar. Al cerrar la lista se obtiene un circuito. En este proyecto, `FollowWP` realiza ese seguimiento con velocidad y giro configurables. `WaypointCircuit` permite usar los hijos como ruta y dibuja el circuito en la vista Scene.

## Proyecto y pruebas manuales en Unity
1. Abrir el proyecto con Unity 6.0.37f1 y abrir `Assets/Scenes/TankRacing.unity`.
2. Pulsar Play: los cuatro tanques siguen la misma ruta cerrada de cinco waypoints, con velocidades distintas. El marcador azul identifica cada punto.
3. Cambiar `Move Speed` en cualquiera de los componentes `FollowWP` para comparar las velocidades; detener Play y guardar si se desean conservar cambios.
4. Seleccionar `WaypointCircuit` para ver el circuito; mover sus hijos cambia la ruta.
5. Ajustar `turnSpeed` o `arrivalDistance` para variar giros y distancia de llegada.

## Siguiente ejercicio: circuito y tracker
`WaypointCircuit`, `CircuitTracker` y `FollowTracker` implementan el segundo enfoque explicado por Unity Learn: un tracker recorre una curva Catmull-Rom muestreada y los vehículos lo persiguen manteniendo una distancia. Estos scripts se pueden asignar a un objeto tracker visible y a los tanques para probar la alternativa cuando el seguimiento directo genera problemas por física.

## Recursos y referencias
- Starter entregado: `S0501StarterU6_20260320_143112.zip` (TankRacing, Tank.fbx, PalmTree.fbx, materiales Sand/Green/Red/Yellow/Blue y datos de iluminación).
- Unity Learn, [Waypoints](https://learn.unity.com/tutorial/waypoints?uv=2019.4&courseId=5dd851beedbc2a1bf7b72bed&projectId=5e0b6dd4edbc2a00200e3641).
- Unity Learn, [Following a Circuit & Tracker](https://learn.unity.com/tutorial/following-a-circuit): alternativa con un tracker cuando la física hace inestable el seguimiento directo.
- Millington, I. y Funge, J. (2019). *Artificial Intelligence for Games*, 3.ª ed., CRC Press.

> La sesión automatizada de Unity en el equipo no pudo obtener una licencia del Editor (License Client devolvió Connection Refused). Por eso, la validación Play debe completarse al abrir el proyecto en un Editor con licencia; no se declara como probada aquí.
