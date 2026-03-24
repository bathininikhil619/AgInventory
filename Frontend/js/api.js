

// const API_BASE = 'https://localhost:7015/api';
const API_BASE = 'https://aginventory-api-hggwgbgebcgfg8dj.westus3-01.azurewebsites.net/api';

const API = {
    get: function(endpoint) {
        return $.ajax({
            url: API_BASE + endpoint,
            method: 'GET',
            contentType: 'application/json'
        });
    },
    post: function(endpoint, data) {
        return $.ajax({
            url: API_BASE + endpoint,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        });
    }
};

function showAlert(message, type, containerId) {
    const html = `<div class="alert alert-${type}">${message}</div>`;
    $('#' + containerId).html(html);
    setTimeout(() => $('#' + containerId).html(''), 3000);
}

function formatDate(dateString) {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
}

function stockBadge(status) {
    if (status === 'LOW STOCK') return '<span class="badge badge-danger">Low Stock</span>';
    return '<span class="badge badge-success">OK</span>';
}