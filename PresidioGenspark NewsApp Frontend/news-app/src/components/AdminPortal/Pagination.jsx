import React from 'react';

const Pagination = ({ pageNumber, pageSize, totalCount, onPageChange, isLoading }) => {
    const totalPages = Math.ceil(totalCount / pageSize);

    const handleNextPage = () => {
        if (pageNumber < totalPages) {
            onPageChange(pageNumber + 1);
        }
    };

    const handlePreviousPage = () => {
        if (pageNumber > 1) {
            onPageChange(pageNumber - 1);
        }
    };

    const handleFirstPage = () => {
        if (pageNumber !== 1) {
            onPageChange(1);
        }
    };

    const handleLastPage = () => {
        if (pageNumber !== totalPages) {
            onPageChange(totalPages);
        }
    };

    const handlePageClick = (page) => {
        if (page !== pageNumber) {
            onPageChange(page);
        }
    };

    const renderPageNumbers = () => {
        const pageNumbers = [];
        const maxPagesToShow = 5;

        if (totalPages <= maxPagesToShow) {
            for (let i = 1; i <= totalPages; i++) {
                pageNumbers.push(i);
            }
        } else {
            let startPage = Math.max(1, pageNumber - Math.floor(maxPagesToShow / 2));
            let endPage = startPage + maxPagesToShow - 1;

            if (endPage > totalPages) {
                endPage = totalPages;
                startPage = endPage - maxPagesToShow + 1;
            }

            for (let i = startPage; i <= endPage; i++) {
                pageNumbers.push(i);
            }
        }

        return pageNumbers.map((page) => (
            <button
                key={page}
                onClick={() => handlePageClick(page)}
                className={page === pageNumber ? 'active' : ''}
            >
                {page}
            </button>
        ));
    };

    return (
        <div className="pagination-controls">
            <button onClick={handleFirstPage} disabled={pageNumber === 1 || isLoading}>
                First
            </button>
            <button onClick={handlePreviousPage} disabled={pageNumber === 1 || isLoading}>
                Previous
            </button>
            {renderPageNumbers()}
            <button onClick={handleNextPage} disabled={pageNumber >= totalPages || isLoading}>
                Next
            </button>
            <button onClick={handleLastPage} disabled={pageNumber === totalPages || isLoading}>
                Last
            </button>
            {isLoading && <span>Loading...</span>}
        </div>
    );
};

export default Pagination;
