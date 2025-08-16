window.addEventListener('DOMContentLoaded', function () {
    $(document).on('click', '.add-btn', function () {
        const workSiteId = $('#workSiteDropDown').val();
        if (workSiteId == 0 || workSiteId == null) {
            alert('Prego seleziona il cantiere!');
        }
        else {
            $.ajax({
                url: '/Purchases/Create',
                type: 'GET',
                data: { workSiteId: workSiteId },
                success: function (html) {
                    $("#createContent").html(html);
                    $("#createModal").modal("show");
                    $.validator.unobtrusive.parse('#createForm');
                }
            });
        }
    });
    var table = $('#mainTable').DataTable({
        ajax: {
            url: "/Purchases/GetPurchaseList",
            type: "POST",
            data: function (d) {
                d.workSiteDropDown = $('#workSiteDropDown').val(); 
                return d;
            }
        },
        columns: [
            { title: 'No', data: 'no', name: 'No', orderable: false, searchable: false },
            { title: 'Id', data: 'id', name: 'Id', visible: false, searchable: false, orderable: false },
            { title: 'WorkSite', data: 'workSiteName', name: 'WorkSite.Name' },
            { title: 'Material', data: 'materialName', name: 'Material.Name' },
            { title: 'Supplier', data: 'supplierName', name: 'Supplier.UserName' },
            { title: 'Quantity', data: 'quantity', name: 'Quantity' },
            { title: 'Amount', data: 'amount', name: 'Amount' },
            { title: 'Date', data: 'dateDoc', name: 'DateDoc' },
            {
                title: 'DocNumber', data: 'docNumber', name: 'DocNumber' },
            {
                title: 'Edit',
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    let buttons = `
                                    <button class="btn btn-sm btn-warning edit-btn" data-id="${row.id}">
                                        <i class="bi bi-pencil"></i>
                                    </button>`;

                    if (myRole == "Admin") {
                        buttons += `
                                    <button class="btn btn-sm btn-danger delete-btn" data-id="${row.id}">
                                        <i class="bi bi-trash"></i>
                                    </button>`;
                    }
                    return buttons;
                }
            }
        ],
        columnDefs: [
            { targets: 7, className: 'min-width-date' } 
        ],
        initComplete: function () {
            addColumnToggleDropdown(this.api());
        },
        drawCallback: function (settings) {
            // Remove any previous total row
            $('#mainTable tbody tr.summary-row').remove();

            var response = settings.json;
            var totalQuantity = response.count || 0;
            var totalAmount = response.sum || 0;

            const formattedAmount = new Intl.NumberFormat('it-IT', {
                style: 'currency',
                currency: 'EUR'
            }).format(parseFloat(totalAmount));

            // Construct a new summary row
            var summaryRow = `
            <tr class="summary-row table-info">
                <td colspan="4" class="text-end"><strong>Total:</strong></td>
                <td><strong>${parseFloat(totalQuantity)}</strong></td>
                <td><strong>${formattedAmount}</strong></td>
                <td colspan="3"></td>
            </tr>`;

            // Append the summary row to the table body
            $('#mainTable tbody').append(summaryRow);
        }
    });
});

