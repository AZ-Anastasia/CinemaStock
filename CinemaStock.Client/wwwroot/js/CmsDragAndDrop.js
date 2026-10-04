// Хранилище обработчиков для очистки памяти
const activeHandlers = new Map();

export function initialize(areaElement, inputElement) {
    if (!areaElement || !inputElement) return;

    const eventListenersToRemove = [];
    const registerListener = (target, event, callback) => {
        target.addEventListener(event, callback, false);
        eventListenersToRemove.push({ target, event, callback });
    };
    const preventDefaults = (e) => {
        e.preventDefault();
        e.stopPropagation();
    }

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        registerListener(areaElement, eventName, preventDefaults);
    });

    const addHighlight = () => areaElement.classList.add('highlight');
    const removeHighlight = () => areaElement.classList.remove('highlight');

    ['dragenter', 'dragover'].forEach(eventName => {
        registerListener(areaElement, eventName, addHighlight);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        registerListener(areaElement, eventName, removeHighlight);
    });

    const processFile = (file) => {
        if (!file || !file.type.startsWith('image/')) return;

        const reader = new FileReader();
        reader.onload = (event) => {
            inputElement.invokeMethodAsync('HandleExternalFile', event.target.result, file.name)
                .catch(err => console.warn("Сигнал отправлен в уничтоженый Blazor-объек, игнорируем.", err.message));
        };
        reader.readAsDataURL(file);
    };

    // Drop
    const dropHandler = (e) => {
        if (e.dataTransfer.files?.length) {
            processFile(e.dataTransfer.files[0]);
        }
    };

    registerListener(areaElement, 'drop', dropHandler);

    // Paste
    const pasteHandler = (e) => {
        if (e.clipboardData && e.clipboardData.files?.length) {
            processFile(e.clipboardData.files[0]);
        }
    };

    window.addEventListener('paste', pasteHandler, false);

    activeHandlers.set(areaElement, {
        pasteHandler: pasteHandler,
        localListeners: eventListenersToRemove
    });

    return {};
}

export function dispose(areaElement) {
    if (!areaElement) return;

    const handlers = activeHandlers.get(areaElement);
    if (handlers) {
        window.removeEventListener('paste', handlers.pasteHandler, false);

        handlers.localListeners.forEach(listener => {
            listener.target.removeEventListener(listener.event, listener.callback, false);
        });

        activeHandlers.delete(areaElement);
    }
}

export function openExplorer(inputElement) {
    if (inputElement) {
        inputElement.click();
    }
}

export function createPreviewUrl(inputElement) {
    if (inputElement && inputElement.files && inputElement.files[0]) {
        return URL.createObjectURL(inputElement.files[0]);
    }
    return '';
}
export function revokePreviewUrl(url) {
    if (url && url.startsWith('blob:')) {
        URL.revokeObjectURL(url);
    }
}