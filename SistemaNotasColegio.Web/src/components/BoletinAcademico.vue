<script setup>
import { computed } from 'vue'
import { formatearFechaLarga, formatearNota } from '../utils/format'
import EstadoAcademicoBadge from './EstadoAcademicoBadge.vue'
import EstadoBadge from './EstadoBadge.vue'

const props = defineProps({
  boletin: { type: Object, required: true }
})

const materias = computed(() =>
  Array.isArray(props.boletin?.materias) ? props.boletin.materias : []
)

const aprobadas = computed(
  () => materias.value.filter((materia) => materia.estado === 'APROBADO').length
)
const reprobadas = computed(
  () => materias.value.filter((materia) => materia.estado === 'REPROBADO').length
)
const incompletas = computed(
  () => materias.value.filter((materia) => materia.estado === 'INCOMPLETO').length
)

const fechaEmision = formatearFechaLarga(new Date())
</script>

<template>
  <article class="boletin-documento">
    <header class="boletin-header">
      <div class="boletin-marca">
        <div class="boletin-logo">
          <i class="bi bi-mortarboard-fill"></i>
        </div>
        <div>
          <div class="boletin-institucion">Sistema de Notas Escolar</div>
          <h2 class="boletin-titulo">BOLETÍN ACADÉMICO</h2>
          <div class="boletin-subtitulo">Registro de calificaciones trimestrales</div>
        </div>
      </div>

      <div class="boletin-emision">
        <span>Fecha de emisión</span>
        <strong>{{ fechaEmision }}</strong>
      </div>
    </header>

    <section class="boletin-datos" aria-label="Datos del estudiante">
      <div class="boletin-dato">
        <span>Cédula</span>
        <strong>{{ boletin.cedula }}</strong>
      </div>
      <div class="boletin-dato">
        <span>Nombres</span>
        <strong>{{ boletin.nombres }}</strong>
      </div>
      <div class="boletin-dato">
        <span>Apellidos</span>
        <strong>{{ boletin.apellidos }}</strong>
      </div>
      <div class="boletin-dato">
        <span>Estado del estudiante</span>
        <EstadoBadge :activo="boletin.activo" />
      </div>
    </section>

    <div class="table-responsive-snc">
      <table class="boletin-tabla">
        <thead>
          <tr>
            <th class="texto-izquierda">Materia</th>
            <th>Primer trimestre</th>
            <th>Segundo trimestre</th>
            <th>Tercer trimestre</th>
            <th>Promedio anual</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="materia in materias" :key="materia.materiaId">
            <td class="materia-nombre">
              {{ materia.materiaNombre }}
              <span class="materia-codigo">{{ materia.materiaCodigo }}</span>
            </td>
            <td>{{ formatearNota(materia.notaPrimerTrimestre) }}</td>
            <td>{{ formatearNota(materia.notaSegundoTrimestre) }}</td>
            <td>{{ formatearNota(materia.notaTercerTrimestre) }}</td>
            <td>
              <strong>{{ formatearNota(materia.promedioAnual) }}</strong>
            </td>
            <td>
              <EstadoAcademicoBadge :estado="materia.estado" />
            </td>
          </tr>
          <tr v-if="materias.length === 0">
            <td colspan="6" class="text-center py-4">
              El estudiante no tiene materias registradas para mostrar.
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="boletin-resumen">
      <span class="resumen-chip">Materias: {{ materias.length }}</span>
      <span class="resumen-chip">Aprobadas: {{ aprobadas }}</span>
      <span class="resumen-chip">Reprobadas: {{ reprobadas }}</span>
      <span class="resumen-chip">Incompletas: {{ incompletas }}</span>
    </div>

    <div class="boletin-leyenda">
      <span><i class="bi bi-info-circle me-1"></i>Nota mínima de aprobación: 51.</span>
      <span>El promedio anual se calcula únicamente al registrar las tres calificaciones.</span>
    </div>

    <div class="boletin-firmas">
      <div class="firma">Secretaría Académica</div>
      <div class="firma">Sello de la institución</div>
    </div>
  </article>
</template>
