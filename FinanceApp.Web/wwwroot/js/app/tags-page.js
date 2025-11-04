/**
 * Логика страницы управления тегами
 */
document.addEventListener('DOMContentLoaded', function() {
    const tagForm = document.getElementById('tag-form');
    const saveTagBtn = document.getElementById('save-tag-btn');
    const addTagModal = new bootstrap.Modal(document.getElementById('addTagModal'));
    
    // Загрузка тегов при открытии страницы
    loadTags();
    
    // Обработчик сохранения тега
    saveTagBtn.addEventListener('click', async function() {
        const name = document.getElementById('tag-name').value.trim();
        const type = parseInt(document.getElementById('tag-type').value);
        const color = document.getElementById('tag-color').value;
        const icon = document.getElementById('tag-icon').value.trim();
        
        if (!name) {
            alert('Введите название тега');
            return;
        }
        
        try {
            debugger;
            await window.tagsService.createTag({
                name: name,
                type: type,
                color: color,
                icon: icon,
                visibility: 'Private'
            });
            
            // Закрываем модальное окно и очищаем форму
            addTagModal.hide();
            tagForm.reset();
            
            // Перезагружаем теги
            loadTags();
            
            showNotification('Тег создан успешно', 'success');
        } catch (error) {
            showNotification('Ошибка при создании тега: ' + (error.data?.message || error.message), 'error');
        }
    });
    
    /**
     * Загрузка всех тегов
     */
    async function loadTags() {
        try {
            // Загружаем теги по типам
            const incomeTags = await window.tagsService.getTags('Income');
            const expenseTags = await window.tagsService.getTags('Expense');
            const transferTags = await window.tagsService.getTags('Transfer');
            
            displayTags('income-tags', incomeTags);
            displayTags('expense-tags', expenseTags);
            displayTags('transfer-tags', transferTags);
        } catch (error) {
            console.error('Error loading tags:', error);
            
            const errorMessage = error.isNetworkError 
                ? '<p class="text-warning">API недоступен</p>' 
                : '<p class="text-danger">Ошибка загрузки</p>';
            
            document.getElementById('income-tags').innerHTML = errorMessage;
            document.getElementById('expense-tags').innerHTML = errorMessage;
            document.getElementById('transfer-tags').innerHTML = errorMessage;
        }
    }
    
    /**
     * Отображение тегов в контейнере
     */
    function displayTags(containerId, tags) {
        const container = document.getElementById(containerId);
        
        if (!tags || tags.length === 0) {
            container.innerHTML = '<p class="text-muted">Нет тегов</p>';
            return;
        }
        
        container.innerHTML = tags.map(tag => createTagElement(tag)).join('');
    }
    
    /**
     * Создание HTML элемента тега
     */
    function createTagElement(tag) {
        const bgColor = tag.color || '#007bff';
        const icon = tag.icon || '🏷️';
        
        return `
            <div class="tag-item" style="background-color: ${bgColor}20; border-left: 3px solid ${bgColor};">
                <span class="tag-icon">${icon}</span>
                <span class="tag-name">${tag.name}</span>
                <span class="tag-actions">
                    <button onclick="deleteTag('${tag.id}')" title="Удалить">
                        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-x"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                    </button>
                </span>
            </div>
        `;
    }
    
    /**
     * Удаление тега
     */
    window.deleteTag = async function(id) {
        if (!confirm('Вы уверены, что хотите удалить этот тег?')) {
            return;
        }
        
        try {
            await window.tagsService.deleteTag(id);
            loadTags();
            showNotification('Тег удален', 'success');
        } catch (error) {
            showNotification('Ошибка при удалении тега: ' + (error.data?.message || error.message), 'error');
        }
    };
    
    /**
     * Показ уведомления
     */
    function showNotification(message, type) {
        // Простое уведомление через alert (можно заменить на toast)
        alert(message);
    }
});

