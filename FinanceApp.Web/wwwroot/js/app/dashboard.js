/**
 * Логика страницы дашборда
 */
document.addEventListener('DOMContentLoaded', function() {
    const operationForm = document.getElementById('operation-form');
    const operationsList = document.getElementById('operations-list');
    const dateInput = document.getElementById('operation-date');
    
    // Устанавливаем текущую дату по умолчанию
    dateInput.valueAsDate = new Date();
    
    // Загрузка данных при открытии страницы
    loadDashboardData();
    
    // Обработчик формы добавления операции
    operationForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const type = parseInt(document.getElementById('operation-type').value);
        const amount = parseFloat(document.getElementById('operation-amount').value);
        const paymentMethod = parseInt(document.getElementById('operation-payment').value);
        const date = document.getElementById('operation-date').value;
        const description = document.getElementById('operation-description').value;
        
        try {
            await window.operationsService.createOperation({
                type: type,
                amount: amount,
                currency: 'RUB',
                paymentMethod: paymentMethod,
                description: description,
                operationDateTime: new Date(date).toISOString(),
                tagIds: []
            });
            
            // Очищаем форму
            operationForm.reset();
            dateInput.valueAsDate = new Date();
            
            // Перезагружаем данные
            loadDashboardData();
            
            showNotification('Операция добавлена успешно', 'success');
        } catch (error) {
            showNotification('Ошибка при добавлении операции: ' + (error.data?.message || error.message), 'error');
        }
    });
    
    /**
     * Загрузка всех данных дашборда
     */
    async function loadDashboardData() {
        const now = new Date();
        const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
        const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0, 23, 59, 59);
        
        try {
            // Загружаем статистику
            const stats = await window.operationsService.getStats(
                startOfMonth.toISOString(),
                endOfMonth.toISOString()
            );
            
            updateStats(stats);
            
            // Загружаем список операций
            const operations = await window.operationsService.getOperations({
                startDate: startOfMonth.toISOString(),
                endDate: endOfMonth.toISOString(),
                page: 1,
                pageSize: 50
            });
            
            displayOperations(operations.items || operations);
        } catch (error) {
            console.error('Error loading dashboard data:', error);
            
            // Различаем типы ошибок
            if (error.isNetworkError) {
                operationsList.innerHTML = '<tr><td colspan="6" class="text-center text-warning">API недоступен. Убедитесь что FinanceApp.Api запущен.</td></tr>';
            } else if (error.status === 401) {
                // 401 обработается автоматически в api-client
                operationsList.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Требуется авторизация</td></tr>';
            } else {
                operationsList.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Ошибка загрузки данных: ' + (error.message || 'Неизвестная ошибка') + '</td></tr>';
            }
        }
    }
    
    /**
     * Обновление статистики
     */
    function updateStats(stats) {
        const rubStats = stats['RUB'] || { totalIncome: 0, totalExpense: 0, balance: 0 };
        
        document.getElementById('total-income').textContent = formatMoney(rubStats.totalIncome);
        document.getElementById('total-expense').textContent = formatMoney(rubStats.totalExpense);
        document.getElementById('total-balance').textContent = formatMoney(rubStats.balance);
    }
    
    /**
     * Отображение списка операций
     */
    function displayOperations(operations) {
        if (!operations || operations.length === 0) {
            operationsList.innerHTML = '<tr><td colspan="6" class="text-center">Нет операций</td></tr>';
            return;
        }
        
        operationsList.innerHTML = operations.map(op => `
            <tr>
                <td><div class="td-content">${formatDate(op.operationDateTime)}</div></td>
                <td><div class="td-content">${getOperationTypeLabel(op.type)}</div></td>
                <td><div class="td-content">${op.description || '-'}</div></td>
                <td><div class="td-content">${getPaymentMethodLabel(op.paymentMethod)}</div></td>
                <td><div class="td-content ${getAmountClass(op.type)}">${formatAmount(op.type, op.money.amount)}</div></td>
                <td>
                    <div class="td-content">
                        <button class="btn btn-sm btn-danger" onclick="deleteOperation('${op.id}')">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-trash-2"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path><line x1="10" y1="11" x2="10" y2="17"></line><line x1="14" y1="11" x2="14" y2="17"></line></svg>
                        </button>
                    </div>
                </td>
            </tr>
        `).join('');
    }
    
    /**
     * Удаление операции
     */
    window.deleteOperation = async function(id) {
        if (!confirm('Вы уверены, что хотите удалить операцию?')) {
            return;
        }
        
        try {
            await window.operationsService.deleteOperation(id);
            loadDashboardData();
            showNotification('Операция удалена', 'success');
        } catch (error) {
            showNotification('Ошибка при удалении операции', 'error');
        }
    };
    
    /**
     * Вспомогательные функции форматирования
     */
    function formatMoney(amount) {
        return new Intl.NumberFormat('ru-RU', {
            style: 'currency',
            currency: 'RUB',
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        }).format(amount);
    }
    
    function formatAmount(type, amount) {
        const sign = type === 'Income' || type === 0 ? '+' : '-';
        return `${sign} ${formatMoney(amount)}`;
    }
    
    function formatDate(dateString) {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        }).format(date);
    }
    
    function getOperationTypeLabel(type) {
        const types = {
            'Income': 'Доход',
            'Expense': 'Расход',
            'Transfer': 'Перевод',
            0: 'Доход',
            1: 'Расход',
            2: 'Перевод'
        };
        return types[type] || type;
    }
    
    function getPaymentMethodLabel(method) {
        const methods = {
            'Cash': 'Наличные',
            'Card': 'Карта',
            'BankTransfer': 'Перевод',
            0: 'Наличные',
            1: 'Карта',
            2: 'Перевод'
        };
        return methods[method] || method;
    }
    
    function getAmountClass(type) {
        return (type === 'Income' || type === 0) ? 'text-success' : 'text-danger';
    }
    
    function showNotification(message, type) {
        // Простое уведомление через alert (можно заменить на toast)
        alert(message);
    }
});

