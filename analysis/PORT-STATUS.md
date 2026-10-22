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
- Empuje nativo de los 11 tipos pasivos de conductas 1/10/28, incluido el muelle:
  cadenas, límites, alturas, soporte, congelación y persistencia por sala. Ver
  `PUSHING-Z80.md` para evidencia y diferencias respecto a las máscaras originales.
- Salto aumentado y compresión/recuperación del muelle; plataformas verticales
  16/18/23 con transporte del personaje y transición por hueco entre 6160 y 6150.
- Persecución corregida de familias 15/33 y orden de consulta aleatoria de 9/12/13.
- Tablas y secuenciador de efectos CPC traducidos; once asignaciones comprobadas
  mediante vectores AY iniciales. Síntesis y prioridades aún no certificadas.
- Gravedad de coleccionables (26→4DCF→4DDC/5029), contacto por volumen y adquisición
  compartida de bolsa/bocina (3FA5). Regresión en sala3060, caída hasta altura3,
  recogida con ambos personajes y disparo de Head tras conseguir munición.

Pendiente para una réplica completa:

- Comparar partidas y tiempos reales con el original: las pruebas numéricas no
  certifican equivalencia de todas las colisiones ni del ciclo completo.
- Completar variantes de cambios de planta/caídas y entradas especiales; final.
- Completar los37 comportamientos: interruptores, desaparición, interacciones
  entre cuerpos y cinta transportadora. Completar inercia y latencia de empuje,
  máscaras y cronometraje compartido de las plataformas verticales.
- Máscaras direccionales/contacto de cuerpos móviles e inicialización exacta de
  cada dirección; el port actual de esas familias es parcial.
- Congelación/persistencia al reconstruir una habitación; estado completo de
  checkpoints y todos los casos de muerte de personajes unidos.
- Suelo original, selección completa de variantes de paredes y composición
  exacta de puertas/objetos, ocultación e interpolación de imagen.
- Menús e interfaz originales; cotejar trazas AY completas, prioridades y música.

Reproducir comprobaciones con `Compilar.cmd`; resultados e imágenes en
`build/actual/verification`. Consultar especialmente `results.json`. No marcar
completeGame o fidelityValidated como verdaderos hasta validar lo anterior.

Corrección de salida de audio: pasos 80 identificados en 2986/2B28; parada
selectiva que no corta un salto. Tres bloques waveOut en cola mediante evento
impiden los huecos de la salida anterior de un solo bloque. Medición real antes
y después en `audio-output-verification.json`; no se ha alterado el reloj AY ni
se han acelerado artificialmente las tablas. El secuenciador conserva 50 Hz.
