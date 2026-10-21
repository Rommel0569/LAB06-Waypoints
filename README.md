# LAB06 — Waypoints

Proyecto de circuitos con waypoint AI, vehículos físicos y selector de modo. Abrir la carpeta del proyecto desde Unity Hub con **Unity 6 `6000.3.17f1`**.

## Inicio rápido

- `Assets/Scenes/MainMenu.unity` es la primera escena del Build Settings. Pulsa Play para abrir el selector con dos modos.
- Elige **Modo tanques** para ver los cuatro tanques recorriendo una secuencia cerrada de nueve waypoints.
- Elige **Carrera de autos** para correr en el circuito costero. Conduce el auto Azure con `WASD` o las flechas; `Espacio` aplica el freno de mano.
- Usa el HUD para cambiar entre seguimiento por waypoints y tracker, alternar la cámara, cambiar de circuito o volver al menú.

## Escenas y contenido

- `Assets/Scenes/MainMenu.unity`: selector visual entre los dos circuitos.
- `Assets/Scenes/TankRacing.unity`: escenario original de tanques y ruta de nueve waypoints.
- `Assets/Scenes/CarCircuit.unity`: circuito costero de autos; al abrirla, el contenido visible se genera y guarda en modo de edición, sin Play.

La pista de autos incluye colisionadores, tres rivales en carriles separados, una meta con contador de vueltas y cámara seleccionable. El informe con actividades, ejercicios propuestos, cuestionario y espacios para capturas está en [`Assets/Documentation/LAB06_Waypoints.md`](Assets/Documentation/LAB06_Waypoints.md) y [`Assets/Documentation/Informe_Laboratorio_06_Waypoints.docx`](Assets/Documentation/Informe_Laboratorio_06_Waypoints.docx).
