// Управление тегами
const API_BASE_URL = 'https://localhost:7051/api';
let currentEditingTagId = null;

document.addEventListener('DOMContentLoaded', function() {
    loadAllTags();
    
    // Обработчик сохранения нового тега
    document.getElementById('save-tag-btn').addEventListener('click', saveTag);
});

async function loadAllTags() {
    try {
        await loadTagsByType('Income', 'income-tags');
        await loadTagsByType('Expense', 'expense-tags');
        await loadTagsByType('Transfer', 'transfer-tags');
    } catch (error) {
        console.error('Ошибка загрузки тегов:', error);
        showNotification('Ошибка загрузки тегов', 'error');
    }
}

async function loadTagsByType(type, containerId) {
    const container = document.getElementById(containerId);
    
    try {
        const response = await fetch(`${API_BASE_URL}/Tags/by-type/${type}`, {
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) throw new Error('Ошибка загрузки тегов');

        const tags = await response.json();
        
        if (tags.length === 0) {
            container.innerHTML = '<p class="text-muted">Нет тегов</p>';
            return;
        }

        container.innerHTML = tags.map(tag => createTagElement(tag)).join('');
        
        // Привязываем обработчики к кнопкам
        container.querySelectorAll('.edit-tag-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const tagId = btn.getAttribute('data-tag-id');
                openEditTagModal(tagId);
            });
        });
        
        container.querySelectorAll('.delete-tag-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const tagId = btn.getAttribute('data-tag-id');
                const tagName = btn.getAttribute('data-tag-name');
                deleteTag(tagId, tagName);
            });
        });
    } catch (error) {
        console.error('Ошибка:', error);
        container.innerHTML = '<p class="text-danger">Ошибка загрузки</p>';
    }
}

function createTagElement(tag) {
    const color = tag.color || '#007bff';
    const icon = tag.icon || '🏷️';
    
    return `
        <div class="tag-item" style="background-color: ${color}20; border-left: 3px solid ${color}">
            <span class="tag-icon">${icon}</span>
            <span class="tag-name">${tag.name}</span>
            <span class="tag-actions">
                <button class="edit-tag-btn" data-tag-id="${tag.id}" title="Редактировать">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="delete-tag-btn" data-tag-id="${tag.id}" data-tag-name="${tag.name}" title="Удалить">
                    <i class="fas fa-trash"></i>
                </button>
            </span>
        </div>
    `;
}

async function saveTag() {
    const name = document.getElementById('tag-name').value.trim();
    const type = parseInt(document.getElementById('tag-type').value);
    const color = document.getElementById('tag-color').value;
    const icon = document.getElementById('tag-icon').value.trim();

    if (!name) {
        showNotification('Введите название тега', 'warning');
        return;
    }

    const tagData = {
        name: name,
        type: type,
        color: color || '#007bff',
        icon: icon || '🏷️',
        visibility: 0 // Private
    };

    try {
        let response;
        
        if (currentEditingTagId) {
            // Обновление существующего тега
            response = await fetch(`${API_BASE_URL}/Tags/${currentEditingTagId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getToken()}`
                },
                body: JSON.stringify(tagData)
            });
        } else {
            // Создание нового тега
            response = await fetch(`${API_BASE_URL}/Tags`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${getToken()}`
                },
                body: JSON.stringify(tagData)
            });
        }

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ошибка сохранения тега');
        }

        showNotification(currentEditingTagId ? 'Тег обновлен' : 'Тег создан', 'success');
        
        // Перезагружаем теги СНАЧАЛА
        await loadAllTags();
        
        // Закрываем модальное окно
        const modal = bootstrap.Modal.getInstance(document.getElementById('addTagModal'));
        modal.hide();
        
        // Сбрасываем форму
        document.getElementById('tag-form').reset();
        currentEditingTagId = null;
        
        // Обновляем название модального окна
        document.getElementById('addTagModalLabel').textContent = 'Добавить тег';
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification(error.message, 'error');
    }
}

async function openEditTagModal(tagId) {
    try {
        const response = await fetch(`${API_BASE_URL}/Tags/${tagId}`, {
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) throw new Error('Ошибка загрузки тега');

        const tag = await response.json();
        
        // Заполняем форму
        document.getElementById('tag-name').value = tag.name;
        document.getElementById('tag-type').value = getTypeValue(tag.type);
        document.getElementById('tag-color').value = tag.color || '#007bff';
        document.getElementById('tag-icon').value = tag.icon || '';
        
        // Устанавливаем ID редактируемого тега
        currentEditingTagId = tagId;
        
        // Меняем заголовок модального окна
        document.getElementById('addTagModalLabel').textContent = 'Редактировать тег';
        
        // Открываем модальное окно
        const modal = new bootstrap.Modal(document.getElementById('addTagModal'));
        modal.show();
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification('Ошибка загрузки данных тега', 'error');
    }
}

async function deleteTag(tagId, tagName) {
    if (!confirm(`Вы уверены, что хотите удалить тег "${tagName}"?`)) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/Tags/${tagId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${getToken()}`
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ошибка удаления тега');
        }

        showNotification('Тег удален', 'success');
        await loadAllTags();
    } catch (error) {
        console.error('Ошибка:', error);
        showNotification(error.message, 'error');
    }
}

function getTypeValue(typeString) {
    const types = {
        'Income': 0,
        'Expense': 1,
        'Transfer': 2
    };
    return types[typeString] || 0;
}

function getToken() {
    // Получаем токен из localStorage или cookies
    return localStorage.getItem('access_token') || '';
}

function showNotification(message, type = 'info') {
    // Простая реализация уведомлений
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

// Сброс формы при закрытии модального окна
document.getElementById('addTagModal').addEventListener('hidden.bs.modal', function () {
    document.getElementById('tag-form').reset();
    currentEditingTagId = null;
    document.getElementById('addTagModalLabel').textContent = 'Добавить тег';
});
