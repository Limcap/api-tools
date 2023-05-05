using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Limcap.ApiTools {

	/// <summary>
	/// Somente para uso em atributo de metadados do Swagger, para
	/// especificar o tipo do conteúdo de um retorno específico.
	/// </summary>
	/// <remarks>
	/// Exemplo: O resultado padrão de um <see cref="ApiWebException"/> é:
	/// <br/>StdApiErrorResult{StdApiWebException.Details{object}}
	/// <br/>e pode ser colocado informado como metadado de um método de controller da seguinte forma:
	/// <br/>[ProducesResponseType(424, Type = typeof(StdApiErrorResult{StdApiWebException.Details{object}}))]
	/// </remarks>
	public abstract class ApiErrorResult<T> : ApiResult {
		public string Message { get; private set; }
		public T Details { get; private set; }
		public object Info { get; private set; }
	}
}
