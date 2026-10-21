# Laboratorio 06 — Waypoints y circuitos

**Proyecto:** LAB06 Waypoints

**Editor:** Unity 6, `6000.3.17f1`

**Plataforma:** Windows 64-bit

**Escenas:** `Assets/Scenes/TankRacing.unity` y `Assets/Scenes/CarCircuit.unity`

## 1. Objetivos

El laboratorio busca comprender el uso de waypoints para guiar vehículos por un recorrido y aplicar la técnica para formar un circuito. El proyecto conserva el escenario de tanques del starter y añade una escena de carreras con automóviles. También implementa una variante con un tracker móvil para comparar ambos métodos.

## 2. Desarrollo de las actividades del informe

| Actividad solicitada | Implementación en el proyecto |
|---|---|
| Crear un proyecto 3D en Unity | Proyecto Unity configurado para abrirse con `6000.3.17f1`. |
| Descargar/importar los recursos del tutorial y abrir TankRacing | Se conservaron los recursos del starter entregado: escena TankRacing, modelos FBX de tanque y palmera, terreno, materiales y datos de iluminación. |
| Inspeccionar el escenario | Se preservó el escenario original como base de la escena `TankRacing`. |
| Duplicar y organizar palmeras como referencias | La ruta de tanques contiene nueve palmeras distribuidas alrededor del circuito y nueve objetos `Palm Waypoint 01` a `Palm Waypoint 09`, cada uno asociado a una posición de la ruta. |
| Revisar `FollowWP` y añadirlo al tanque | Los cuatro tanques tienen `FollowWP`, una lista serializada de los nueve puntos y valores de velocidad/configuración. |
| Bloquear/desbloquear el Inspector y asignar puntos | Las referencias de waypoints están serializadas en los cuatro componentes de la escena. No es necesario repetir el arrastre manual al abrirla. |
| Ejecutar, probar y modificar velocidades | La lógica permite variar `moveSpeed`, `turnSpeed`, `arrivalDistance` y el desplazamiento lateral. El código y las referencias se validaron estáticamente; el resultado en Play queda pendiente de ejecución dentro del Editor con licencia, según la limitación de validación indicada abajo. |
| Repetir la configuración en los otros tanques | Blue, Red, Yellow y Green recorren la misma lista cerrada, con inicios, velocidades y carriles diferentes. |

### Cómo funciona el seguimiento directo

`FollowWP` orienta cada vehículo hacia el punto actual, avanza y cambia al siguiente al entrar en el radio de llegada. El último punto vuelve al primero, por lo que la ruta es cerrada. El giro se limita por `turnSpeed`; `laneOffset` separa los vehículos a los lados de la tangente local. `WaypointCircuit` puede obtener una ruta desde sus hijos y dibuja sus segmentos en la vista Scene.

### Ejercicio propuesto 1: Following a Circuit (& Tracker)

La implementación incluye ambos enfoques. `CircuitTracker` recorre una curva Catmull–Rom muestreada a partir de los waypoints; `FollowTracker` persigue la posición del tracker con una distancia de seguimiento. En `CarCircuit`, la tecla **T** (o el botón en pantalla) cambia entre seguimiento directo de waypoints y seguimiento del tracker móvil. La tecla **R** reinicia la escena. Así se puede observar y comparar las dos técnicas en un circuito de carreras.

### Ejercicio propuesto 2: circuito con autos y otros recursos

La escena `CarCircuit` genera en tiempo de ejecución un escenario costero tropical: pista curva cerrada, superficie asfaltada, bordes pintados, pianos, barreras, línea de salida, palmeras, iluminación y cámara. Construye cuatro autos de colores distintos con primitivas de Unity y los mueve por quince puntos de ruta. Los materiales se crean con shaders disponibles en el proyecto; no requiere paquetes externos ni recursos descargados.

Para abrirla, seleccionar `Assets/Scenes/CarCircuit.unity` y pulsar Play. La escena se construye al comenzar Play; en ejecución se usa **T** para cambiar de método y **R** para reiniciar. La escena de tanques es independiente y permanece disponible.

### Ejercicio propuesto 3: informe

Este documento responde los ejercicios, describe la implementación y registra el alcance real de la verificación. Puede entregarse junto con el proyecto en el aula virtual.

## 3. Cuestionario

### 1. ¿Qué son los waypoints?

Son posiciones de referencia ubicadas en el espacio del juego. Ordenadas como una lista, describen por dónde debe pasar un personaje o vehículo. Un agente puede dirigirse al punto activo y cambiar al siguiente cuando llega a una distancia establecida. Si el último punto vuelve al primero, la secuencia forma un circuito cerrado.

### 2. ¿Cómo se implementan en Unity para formar un circuito?

Se crean objetos con `Transform` en cada posición del recorrido, se guardan en orden en un componente y se asigna ese recorrido al vehículo. En cada actualización, el script calcula la dirección al punto activo, orienta el vehículo gradualmente, avanza a la velocidad configurada y detecta la llegada. Al llegar incrementa el índice; al superar el último índice, lo reinicia a cero. En este proyecto esa lógica está en `FollowWP`; la ruta queda representada en la escena por nueve puntos de los tanques y quince de los autos.

## 4. Verificación y límites

- Se compiló el conjunto de scripts C# con el compilador incluido en la instalación de Unity 6 `6000.3.17f1`, usando las bibliotecas del Editor.
- Se revisaron las referencias serializadas de los waypoints, la configuración de escenas de inicio/build y los identificadores de los componentes del proyecto.
- **No se pudo confirmar una ejecución Play de extremo a extremo en esta sesión.** El arranque automatizado del Editor fue rechazado por Unity Licensing (`LicenseClient` devolvió conexión rechazada), así que no se afirma que la simulación se haya probado visualmente. Al abrir el proyecto en tu Unity Hub con la licencia activa, ejecuta ambas escenas para completar esa comprobación visual.

## 5. Referencias

- Unity Learn, [Waypoints](https://learn.unity.com/tutorial/waypoints?uv=2019.4&courseId=5dd851beedbc2a1bf7b72bed&projectId=5e0b6dd4edbc2a00200e3641).
- Unity Learn, [Following a Circuit & Tracker](https://learn.unity.com/tutorial/following-a-circuit).
- Millington, I. y Funge, J. (2019). *Artificial Intelligence for Games* (3.ª ed.). CRC Press.
