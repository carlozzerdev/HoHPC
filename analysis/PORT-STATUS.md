# Estado del motor nativo

El ejecutable no interpreta Z80 ni utiliza un emulador. Los informes de análisis
separan datos observados, traducciones de rutinas y partes pendientes.

Implementado y comprobado:

- Lectura del DSK6128 y recuperación de301 salas,2936 objetos,100 sprites,
  52 coleccionables, ocho paletas y patrones de paredes.
- Colores de sprites derivados del blitter149A y colores directos de paredes1A80.
- Simulación independiente del render: ciclo nominal80ms, pasos y saltos discretos,
  dirección, fotogramas y reflejo, unión/separación de personajes.
- Cajas físicas a partir de flags de cuerpo; bolsa, inventario, vidas y coronas.
- Daño diferido, protección y respawn simplificado.
- Proyectil de Head: permisos, consumo, cuerpo único, cuatro pasos/tick,
  impacto que congela cuerpos elegibles y animación de desaparición.
- PRNG4497 traducido a C#, comprobado con diez vectores independientes.
- Familias de rebote/giro, aleatorias y persecución con colisión nativa.
- Grafo de482 puertas comprobado con retorno y22 parejas de teletransporte.
- Vista de varias habitaciones, nueva partida y ventanas de ambos personajes.

Pendiente para una réplica completa:

- Comparar partidas y tiempos reales con el original: las pruebas numéricas no
  certifican equivalencia de todas las colisiones ni del ciclo completo.
- Cambios de planta por huecos, caídas y entradas especiales; final del juego.
- Completar los37 comportamientos: interruptores, plataformas verticales,
  desaparición, interacciones entre cuerpos, cinta transportadora y empuje.
- Máscaras direccionales/contacto de cuerpos móviles e inicialización exacta de
  cada dirección; el port actual de esas familias es parcial.
- Congelación/persistencia al reconstruir una habitación; estado completo de
  checkpoints y todos los casos de muerte de personajes unidos.
- Suelo original, selección completa de variantes de paredes y composición
  exacta de puertas/objetos, ocultación e interpolación de imagen.
- Menús, interfaz y sonido originales.

Reproducir comprobaciones con `Compilar.cmd`; resultados e imágenes en
`build/verification`. Consultar especialmente `results.json`. No marcar
completeGame o fidelityValidated como verdaderos hasta validar lo anterior.
