// Validaciones de formato alineadas con los DataAnnotations del backend.
// No se replican reglas de negocio (duplicados, permisos, estado, cálculos):
// esas reglas siguen siendo responsabilidad exclusiva de la API.

const REGEX_CEDULA_ESTUDIANTE = /^[0-9A-Za-z-]+$/
const REGEX_NOMBRES = /^[\p{L}\s'\-\.]+$/u
const REGEX_CODIGO_MATERIA = /^[0-9A-Za-z\-_.]+$/
const REGEX_CORREO = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export function esTextoVacio(valor) {
  return !String(valor ?? '').trim()
}

export function longitudInvalida(valor, minimo, maximo) {
  const longitud = String(valor ?? '').trim().length
  return longitud < minimo || longitud > maximo
}

export function esCorreoValido(valor) {
  return REGEX_CORREO.test(String(valor ?? '').trim())
}

export function esEnteroPositivo(valor) {
  const numero = Number(valor)
  return Number.isInteger(numero) && numero > 0
}

export function validarLogin(formulario) {
  const errores = {}

  if (esTextoVacio(formulario.nombreUsuario)) {
    errores.nombreUsuario = 'El nombre de usuario es obligatorio.'
  } else if (String(formulario.nombreUsuario).trim().length > 50) {
    errores.nombreUsuario = 'El nombre de usuario no puede superar los 50 caracteres.'
  }

  if (esTextoVacio(formulario.password)) {
    errores.password = 'La contraseña es obligatoria.'
  } else if (String(formulario.password).length > 100) {
    errores.password = 'La contraseña no puede superar los 100 caracteres.'
  }

  return errores
}

export function validarEstudiante(formulario) {
  const errores = {}
  const cedula = String(formulario.cedula ?? '').trim()
  const nombres = String(formulario.nombres ?? '').trim()
  const apellidos = String(formulario.apellidos ?? '').trim()

  if (!cedula) {
    errores.cedula = 'La cédula es obligatoria.'
  } else if (longitudInvalida(cedula, 5, 20)) {
    errores.cedula = 'La cédula debe tener entre 5 y 20 caracteres.'
  } else if (!REGEX_CEDULA_ESTUDIANTE.test(cedula)) {
    errores.cedula = 'La cédula solo puede contener letras, números y guiones.'
  }

  if (!nombres) {
    errores.nombres = 'Los nombres son obligatorios.'
  } else if (longitudInvalida(nombres, 2, 100)) {
    errores.nombres = 'Los nombres deben tener entre 2 y 100 caracteres.'
  } else if (!REGEX_NOMBRES.test(nombres)) {
    errores.nombres = 'Los nombres solo pueden contener letras, espacios, apóstrofos, puntos y guiones.'
  }

  if (!apellidos) {
    errores.apellidos = 'Los apellidos son obligatorios.'
  } else if (longitudInvalida(apellidos, 2, 100)) {
    errores.apellidos = 'Los apellidos deben tener entre 2 y 100 caracteres.'
  } else if (!REGEX_NOMBRES.test(apellidos)) {
    errores.apellidos = 'Los apellidos solo pueden contener letras, espacios, apóstrofos, puntos y guiones.'
  }

  return errores
}

export function validarMateria(formulario) {
  const errores = {}
  const codigo = String(formulario.codigo ?? '').trim()
  const nombre = String(formulario.nombre ?? '').trim()

  if (!codigo) {
    errores.codigo = 'El código es obligatorio.'
  } else if (longitudInvalida(codigo, 2, 20)) {
    errores.codigo = 'El código debe tener entre 2 y 20 caracteres.'
  } else if (!REGEX_CODIGO_MATERIA.test(codigo)) {
    errores.codigo = 'El código solo puede contener letras, números, guiones, guiones bajos y puntos.'
  }

  if (!nombre) {
    errores.nombre = 'El nombre es obligatorio.'
  } else if (longitudInvalida(nombre, 2, 150)) {
    errores.nombre = 'El nombre debe tener entre 2 y 150 caracteres.'
  }

  return errores
}

export function validarUsuario(formulario, esEdicion = false) {
  const errores = {}
  const cedula = String(formulario.cedula ?? '').trim()
  const nombres = String(formulario.nombres ?? '').trim()
  const apellidos = String(formulario.apellidos ?? '').trim()
  const correo = String(formulario.correo ?? '').trim()
  const nombreUsuario = String(formulario.nombreUsuario ?? '').trim()
  const password = String(formulario.password ?? '')
  const rolId = Number(formulario.rolId)

  if (!cedula) {
    errores.cedula = 'La cédula es obligatoria.'
  } else if (cedula.length > 20) {
    errores.cedula = 'La cédula no puede superar los 20 caracteres.'
  }

  if (!nombres) {
    errores.nombres = 'Los nombres son obligatorios.'
  } else if (nombres.length > 100) {
    errores.nombres = 'Los nombres no pueden superar los 100 caracteres.'
  }

  if (!apellidos) {
    errores.apellidos = 'Los apellidos son obligatorios.'
  } else if (apellidos.length > 100) {
    errores.apellidos = 'Los apellidos no pueden superar los 100 caracteres.'
  }

  if (!correo) {
    errores.correo = 'El correo es obligatorio.'
  } else if (!esCorreoValido(correo)) {
    errores.correo = 'El correo no tiene un formato válido.'
  } else if (correo.length > 150) {
    errores.correo = 'El correo no puede superar los 150 caracteres.'
  }

  if (!nombreUsuario) {
    errores.nombreUsuario = 'El nombre de usuario es obligatorio.'
  } else if (nombreUsuario.length > 50) {
    errores.nombreUsuario = 'El nombre de usuario no puede superar los 50 caracteres.'
  }

  if (!esEdicion || password) {
    if (!password) {
      errores.password = 'La contraseña es obligatoria.'
    } else if (password.length < 8) {
      errores.password = 'La contraseña debe tener al menos 8 caracteres.'
    } else if (password.length > 100) {
      errores.password = 'La contraseña no puede superar los 100 caracteres.'
    }
  }

  if (![1, 2].includes(rolId)) {
    errores.rolId = 'Seleccione el rol de Secretaría o Profesor.'
  }

  return errores
}

export function validarAsignacion(formulario) {
  const errores = {}

  if (!esEnteroPositivo(formulario.profesorId)) {
    errores.profesorId = 'Seleccione un profesor.'
  }

  if (!esEnteroPositivo(formulario.materiaId)) {
    errores.materiaId = 'Seleccione una materia.'
  }

  return errores
}

export function validarNota(valor) {
  if (valor === null || valor === undefined || String(valor).trim() === '') {
    return 'La nota es obligatoria.'
  }

  const numero = Number(valor)
  if (Number.isNaN(numero)) return 'La nota debe ser un número.'
  if (numero < 0 || numero > 100) return 'La nota debe estar entre 0 y 100.'
  if (/[.,]/.test(String(valor))) {
    const decimales = String(valor).split(/[.,]/)[1] ?? ''
    if (decimales.length > 2) return 'La nota admite como máximo dos decimales.'
  }

  return ''
}

export function validarCalificacion(formulario) {
  const errores = {}

  if (!esEnteroPositivo(formulario.estudianteId)) {
    errores.estudianteId = 'Ingrese un identificador de estudiante válido.'
  }

  if (!esEnteroPositivo(formulario.materiaId)) {
    errores.materiaId = 'Seleccione una materia asignada.'
  }

  if (![1, 2, 3].includes(Number(formulario.trimestre))) {
    errores.trimestre = 'Seleccione el trimestre.'
  }

  const errorNota = validarNota(formulario.nota)
  if (errorNota) errores.nota = errorNota

  return errores
}

export function primerError(errores) {
  const valores = Object.values(errores ?? {})
  return valores.length ? valores[0] : ''
}
