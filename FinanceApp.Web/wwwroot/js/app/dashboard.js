// Управление операциями на дашборде
const API_BASE_URL = 'https://localhost:7051/api';
let currentEditingOperationId = null;
let allTags = []; // Хранилище всех доступных тегов
let selectedTagIds = []; // Выбранные теги для текущей операции

document.addEventListener('DOMContentLoaded', function() {
    // Загружаем теги
    loadAllTags();
    // Устанавливаем текущую дату
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('operation-date').value = today;
    
    // Загружаем данные
    loadDashboardData();
    loadOperations();
    
    // Обработчик формы добавления операции
    document.getElementById('operation-form').addEventListener('submit', handleOperationSubmit);
    
    // Обработчик кнопки отмены редактирования
    document.getElementById('cancel-edit-btn').addEventListener('click', cancelEdit);
    
    // Обработчики для модального окна редактирования
    document.getElementById('save-operation-modal-btn').addEventListener('click', saveOperationFromModal);
    document.getElementById('delete-operation-modal-btn').addEventListener('click', deleteOperationFromModal);
});

// Загрузка всех доступных тегов
async function loadAllTags() {
    try {
        const response = await fetch(`${API_BASE_URL}/Tags`, {
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) {
            console.warn('Не удалось загрузить теги');
            return;
        }

        allTags = await response.json();
        console.log('✅ Загружено тегов:', allTags.length);
    } catch (error) {
        console.error('Ошибка загрузки тегов:', error);
    }
}

async function loadDashboardData() {
    try {
        // Проверяем наличие токена
        const token = getToken();
        if (!token) {
            showAuthError();
            return;
        }

        // Получаем статистику за текущий месяц
        const now = new Date();
        const startDate = new Date(now.getFullYear(), now.getMonth(), 1).toISOString();
        const endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString();
        
        console.log('📊 Загрузка статистики:', {
            url: `${API_BASE_URL}/Operations/stats`,
            startDate,
            endDate,
            hasToken: !!token
        });
        
        const response = await fetch(
            `${API_BASE_URL}/Operations/stats?startDate=${startDate}&endDate=${endDate}`,
            {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            }
        );

        console.log('📊 Ответ статистики:', response.status, response.statusText);

        if (response.status === 401) {
            showAuthError();
            return;
        }

        if (!response.ok) {
            const errorText = await response.text();
            console.error('❌ Ошибка ответа:', errorText);
            throw new Error(`Ошибка загрузки статистики: ${response.status}`);
        }

        const stats = await response.json();
        console.log('✅ Статистика загружена:', stats);
        
        // Обновляем статистику (предполагаем RUB)
        if (stats.RUB) {
            document.getElementById('total-income').textContent = 
                `${formatMoney(stats.RUB.totalIncome)} ₽`;
            document.getElementById('total-expense').textContent = 
                `${formatMoney(stats.RUB.totalExpense)} ₽`;
            document.getElementById('total-balance').textContent = 
                `${formatMoney(stats.RUB.balance)} ₽`;
        } else {
            // Если нет данных по RUB, показываем нули
            document.getElementById('total-income').textContent = '0.00 ₽';
            document.getElementById('total-expense').textContent = '0.00 ₽';
            document.getElementById('total-balance').textContent = '0.00 ₽';
        }
    } catch (error) {
        console.error('❌ Ошибка загрузки статистики:', error);
        showNotification('Ошибка загрузки статистики: ' + error.message, 'error');
    }
}

async function loadOperations() {
    const tbody = document.getElementById('operations-list');
    
    try {
        // Проверяем наличие токена
        const token = getToken();
        if (!token) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-warning">⚠️ Необходима авторизация. <a href="/Account/Login">Войти</a></td></tr>';
            showAuthError();
            return;
        }

        console.log('📋 Загрузка операций:', {
            url: `${API_BASE_URL}/Operations`,
            hasToken: !!token
        });

        const response = await fetch(`${API_BASE_URL}/Operations?page=1&pageSize=20`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        console.log('📋 Ответ операций:', response.status, response.statusText);

        if (response.status === 401) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-warning">⚠️ Сессия истекла. <a href="/Account/Login">Войти заново</a></td></tr>';
            showAuthError();
            return;
        }

        if (!response.ok) {
            const errorText = await response.text();
            console.error('❌ Ошибка ответа операций:', errorText);
            throw new Error(`Ошибка загрузки операций: ${response.status}`);
        }

        const result = await response.json();
        console.log('✅ Операции загружены:', result);
        
        const operations = result.items || [];
        
        if (operations.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">📭 Нет операций. Добавьте первую операцию выше!</td></tr>';
            return;
        }

        tbody.innerHTML = operations.map(op => createOperationRow(op)).join('');
        
        // Привязываем обработчики
        tbody.querySelectorAll('.edit-operation-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation(); // Предотвращаем всплытие события
                const operationId = btn.getAttribute('data-operation-id');
                openEditOperationModal(operationId);
            });
        });
        
        tbody.querySelectorAll('.delete-operation-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation(); // Предотвращаем всплытие события
                const operationId = btn.getAttribute('data-operation-id');
                deleteOperation(operationId);
            });
        });
    } catch (error) {
        console.error('Ошибка:', error);
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Ошибка загрузки</td></tr>';
    }
}

function createOperationRow(operation) {
    const isIncome = operation.type === 'Income';
    const typeClass = isIncome ? 'income' : 'expense';
    const typeIcon = isIncome ? '<i class="fas fa-arrow-up"></i>' : '<i class="fas fa-arrow-down"></i>';
    const date = new Date(operation.operationDateTime).toLocaleDateString('ru-RU');
    const paymentMethodClass = getPaymentMethodClass(operation.paymentMethod);
    
    return `
        <tr data-operation-id="${operation.id}" data-operation='${JSON.stringify(operation).replace(/'/g, "&apos;")}'>
            <td><div class="td-content operation-date">${date}</div></td>
            <td>
                <div class="td-content">
                    <span class="operation-type-badge ${typeClass}">
                        ${typeIcon}
                        <span>${getTypeLabel(operation.type)}</span>
                    </span>
                </div>
            </td>
            <td>
                <div class="td-content operation-description ${operation.description ? '' : 'empty'}">
                    ${operation.description || 'Без описания'}
                </div>
            </td>
            <td>
                <div class="td-content">
                    <span class="payment-method-badge ${paymentMethodClass}">
                        ${getPaymentMethodLabel(operation.paymentMethod)}
                    </span>
                </div>
            </td>
            <td>
                <div class="td-content">
                    <span class="operation-amount ${typeClass}">
                        ${formatMoney(operation.money.amount)} ${operation.money.currency}
                    </span>
                </div>
            </td>
                <td>
                    <div class="td-content">
                    <div class="operation-actions">
                        <button class="operation-action-btn edit-btn edit-operation-btn" 
                                data-operation-id="${operation.id}" 
                                title="Редактировать">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="operation-action-btn delete-btn delete-operation-btn" 
                                data-operation-id="${operation.id}" 
                                title="Удалить">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                    </div>
                </td>
            </tr>
    `;
}

async function handleOperationSubmit(e) {
    e.preventDefault();
    
    const type = parseInt(document.getElementById('operation-type').value);
    const amount = parseFloat(document.getElementById('operation-amount').value);
    const paymentMethod = parseInt(document.getElementById('operation-payment').value);
    const date = document.getElementById('operation-date').value;
    const description = document.getElementById('operation-description').value.trim();

    if (!amount || amount <= 0) {
        showNotification('Введите корректную сумму', 'warning');
        return;
    }

    const operationData = {
        type: type,
        amount: amount,
        currency: 'RUB',
        paymentMethod: paymentMethod,
        operationDateTime: new Date(date).toISOString(),
        description: description || null
    };

    try {
        let response;
        
        if (currentEditingOperationId) {
            // Обновление существующей операции
            response = await fetch(`${API_BASE_URL}/Operations/${currentEditingOperationId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getToken()}`
                },
                body: JSON.stringify(operationData)
            });
        } else {
            // Создание новой операции
            response = await fetch(`${API_BASE_URL}/Operations`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getToken()}`
                },
                body: JSON.stringify(operationData)
            });
        }

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ошибка сохранения операции');
        }

        showNotification(
            currentEditingOperationId ? 'Операция обновлена' : 'Операция добавлена', 
            'success'
        );
        
        // Сбрасываем форму
        document.getElementById('operation-form').reset();
        document.getElementById('operation-date').value = new Date().toISOString().split('T')[0];
        currentEditingOperationId = null;
        
        // Обновляем кнопку формы
        const submitBtn = document.querySelector('#operation-form button[type="submit"]');
        submitBtn.textContent = 'Добавить';
        submitBtn.className = 'btn btn-primary';
        
        // Скрываем кнопку отмены
        document.getElementById('cancel-edit-btn').style.display = 'none';
        
        // Перезагружаем данные
        await loadDashboardData();
        await loadOperations();
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification(error.message, 'error');
    }
}

async function openEditOperationModal(operationId) {
    try {
        // Перезагружаем теги перед открытием (для актуальности данных)
        await loadAllTags();
        
        const response = await fetch(`${API_BASE_URL}/Operations/${operationId}`, {
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) throw new Error('Ошибка загрузки операции');

        const operation = await response.json();
        
        // Заполняем форму в модальном окне
        document.getElementById('edit-operation-type').value = getTypeValue(operation.type);
        document.getElementById('edit-operation-amount').value = operation.money.amount;
        document.getElementById('edit-operation-payment').value = getPaymentMethodValue(operation.paymentMethod);
        
        const date = new Date(operation.operationDateTime);
        document.getElementById('edit-operation-date').value = date.toISOString().split('T')[0];
        
        document.getElementById('edit-operation-description').value = operation.description || '';
        document.getElementById('edit-operation-notes').value = operation.notes || '';
        
        // Загружаем и отображаем теги операции
        selectedTagIds = operation.tags ? operation.tags.map(t => t.id) : [];
        renderTagsSelector(selectedTagIds);
        
        // Устанавливаем ID редактируемой операции
        currentEditingOperationId = operationId;
        
        // Открываем модальное окно
        const modal = new bootstrap.Modal(document.getElementById('editOperationModal'));
        modal.show();
        
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification('Ошибка загрузки операции', 'error');
    }
}

// Отображение селектора тегов
function renderTagsSelector(preselectedIds = []) {
    const container = document.getElementById('edit-operation-tags-container');
    
    if (!allTags || allTags.length === 0) {
        container.innerHTML = '<span class="text-muted">Теги не найдены</span>';
        return;
    }
    
    container.innerHTML = '';
    
    allTags.forEach(tag => {
        const isSelected = preselectedIds.includes(tag.id);
        
        const badge = document.createElement('span');
        badge.className = `badge ${isSelected ? 'bg-primary' : 'bg-secondary'} cursor-pointer`;
        badge.style.cursor = 'pointer';
        badge.style.padding = '8px 12px';
        badge.style.fontSize = '14px';
        badge.style.marginRight = '8px';
        badge.style.marginBottom = '8px';
        badge.innerHTML = `
            ${tag.icon || '🏷️'} ${tag.name}
        `;
        
        badge.addEventListener('click', function() {
            toggleTag(tag.id, badge);
        });
        
        container.appendChild(badge);
    });
    
    // Обновляем скрытое поле
    document.getElementById('edit-operation-tags').value = JSON.stringify(preselectedIds);
}

// Переключение выбора тега
function toggleTag(tagId, badgeElement) {
    const index = selectedTagIds.indexOf(tagId);
    
    if (index > -1) {
        // Убираем тег
        selectedTagIds.splice(index, 1);
        badgeElement.className = 'badge bg-secondary cursor-pointer';
        badgeElement.style.cursor = 'pointer';
        badgeElement.style.padding = '8px 12px';
        badgeElement.style.fontSize = '14px';
        badgeElement.style.marginRight = '8px';
        badgeElement.style.marginBottom = '8px';
    } else {
        // Добавляем тег
        selectedTagIds.push(tagId);
        badgeElement.className = 'badge bg-primary cursor-pointer';
        badgeElement.style.cursor = 'pointer';
        badgeElement.style.padding = '8px 12px';
        badgeElement.style.fontSize = '14px';
        badgeElement.style.marginRight = '8px';
        badgeElement.style.marginBottom = '8px';
    }
    
    // Обновляем скрытое поле
    document.getElementById('edit-operation-tags').value = JSON.stringify(selectedTagIds);
    
    console.log('✅ Выбранные теги:', selectedTagIds);
}

async function deleteOperation(operationId) {
    if (!confirm('Вы уверены, что хотите удалить эту операцию?')) {
            return;
        }
        
        try {
        const response = await fetch(`${API_BASE_URL}/Operations/${operationId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) throw new Error('Ошибка удаления операции');

            showNotification('Операция удалена', 'success');
        await loadDashboardData();
        await loadOperations();
        } catch (error) {
        console.error('Ошибка:', error);
        showNotification(error.message, 'error');
    }
}

// Вспомогательные функции
function getTypeValue(typeString) {
    const types = {
        'Income': 0,
        'Expense': 1
    };
    return types[typeString] || 0;
}

function getTypeLabel(typeString) {
    const labels = {
        'Income': 'Доход',
        'Expense': 'Расход',
        'Transfer': 'Перевод'
    };
    return labels[typeString] || typeString;
}

function getPaymentMethodValue(methodString) {
    const methods = {
        'Cash': 0,
        'Card': 1,
        'BankTransfer': 2
    };
    return methods[methodString] || 0;
}

function getPaymentMethodLabel(methodString) {
    const labels = {
        'Cash': 'Наличные',
        'Card': 'Карта',
        'BankTransfer': 'Перевод'
    };
    return labels[methodString] || methodString;
}

function getPaymentMethodClass(methodString) {
    const classes = {
        'Cash': 'cash',
        'Card': 'card',
        'BankTransfer': 'transfer'
    };
    return classes[methodString] || 'card';
}

    function formatMoney(amount) {
        return new Intl.NumberFormat('ru-RU', {
        minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(amount);
    }
    
function getToken() {
    const token = localStorage.getItem('access_token') || '';
    if (!token) {
        console.warn('⚠️ Токен авторизации не найден в localStorage');
    }
    return token;
}

function showAuthError() {
    const message = `
        <div class="alert alert-warning alert-dismissible fade show" role="alert">
            <h5>⚠️ Требуется авторизация</h5>
            <p>Для просмотра операций необходимо войти в систему.</p>
            <hr>
            <p class="mb-0">
                <strong>Что делать:</strong><br>
                1. <a href="/Account/Login" class="alert-link">Войдите в систему</a><br>
                2. Или используйте <a href="/TEST_API.html" class="alert-link">тестовую страницу</a> для получения токена<br>
                3. Сохраните токен: <code>localStorage.setItem('access_token', 'ваш_токен')</code>
            </p>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Показываем сообщение вверху страницы
    const container = document.querySelector('.row.layout-top-spacing');
    if (container) {
        const alertDiv = document.createElement('div');
        alertDiv.className = 'col-12';
        alertDiv.innerHTML = message;
        container.insertBefore(alertDiv, container.firstChild);
    }
    
    showNotification('Необходима авторизация', 'warning');
    console.error('❌ Ошибка авторизации: токен отсутствует или недействителен');
}

function cancelEdit() {
    // Сбрасываем форму
    document.getElementById('operation-form').reset();
    document.getElementById('operation-date').value = new Date().toISOString().split('T')[0];
    currentEditingOperationId = null;
    
    // Обновляем кнопку формы
    const submitBtn = document.querySelector('#operation-form button[type="submit"]');
    submitBtn.textContent = 'Добавить';
    submitBtn.className = 'btn btn-primary';
    
    // Скрываем кнопку отмены
    document.getElementById('cancel-edit-btn').style.display = 'none';
    
    showNotification('Редактирование отменено', 'info');
}

async function saveOperationFromModal() {
    if (!currentEditingOperationId) {
        showNotification('Ошибка: ID операции не найден', 'error');
        return;
    }

    const type = parseInt(document.getElementById('edit-operation-type').value);
    const amount = parseFloat(document.getElementById('edit-operation-amount').value);
    const paymentMethod = parseInt(document.getElementById('edit-operation-payment').value);
    const date = document.getElementById('edit-operation-date').value;
    const description = document.getElementById('edit-operation-description').value.trim();
    const notes = document.getElementById('edit-operation-notes').value.trim();

    if (!amount || amount <= 0) {
        showNotification('Введите корректную сумму', 'warning');
        return;
    }

    const operationData = {
        type: type,
        amount: amount,
        currency: 'RUB',
        paymentMethod: paymentMethod,
        operationDateTime: new Date(date).toISOString(),
        description: description || null,
        notes: notes || null,
        tagIds: selectedTagIds // Добавляем выбранные теги
    };
    
    console.log('Отправка данных операции:', operationData);

    try {
        const response = await fetch(`${API_BASE_URL}/Operations/${currentEditingOperationId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${getToken()}`
            },
            body: JSON.stringify(operationData)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ошибка обновления операции');
        }

        showNotification('Операция успешно обновлена', 'success');
        
        // Закрываем модальное окно
        const modal = bootstrap.Modal.getInstance(document.getElementById('editOperationModal'));
        modal.hide();
        
        // Сбрасываем ID и выбранные теги
        currentEditingOperationId = null;
        selectedTagIds = [];
        
        // Перезагружаем теги (для обновления при изменениях)
        await loadAllTags();
        
        // Перезагружаем данные
        await loadDashboardData();
        await loadOperations();
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification(error.message, 'error');
    }
}

async function deleteOperationFromModal() {
    if (!currentEditingOperationId) {
        showNotification('Ошибка: ID операции не найден', 'error');
        return;
    }

    if (!confirm('Вы действительно хотите удалить эту операцию?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/Operations/${currentEditingOperationId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) throw new Error('Ошибка удаления операции');

        showNotification('Операция удалена', 'success');
        
        // Закрываем модальное окно
        const modal = bootstrap.Modal.getInstance(document.getElementById('editOperationModal'));
        modal.hide();
        
        // Сбрасываем ID
        currentEditingOperationId = null;
        
        // Перезагружаем данные
        await loadDashboardData();
        await loadOperations();
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification(error.message, 'error');
    }
}

function showNotification(message, type = 'info') {
    const alertClass = type === 'success' ? 'alert-success' : 
                      type === 'error' ? 'alert-danger' : 
                      type === 'warning' ? 'alert-warning' : 'alert-info';
    
    const notification = document.createElement('div');
    notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 3000);
}

