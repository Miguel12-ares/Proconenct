using System.Collections.Generic;

namespace ProConnect.Application.DTOs.Shared
{
    /// <summary>
    /// DTO para resultados paginados
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la lista</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Lista de elementos
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Número total de elementos
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Página actual
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Número total de páginas
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indica si hay página anterior
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Indica si hay página siguiente
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;
    }
} 