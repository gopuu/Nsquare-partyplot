(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined'
      ? module.exports = factory()
      : typeof define === 'function' && define.amd
      ? define(factory)
      : (global = typeof globalThis !== 'undefined' ? globalThis : global || self, global.config = factory());
})(this, function () {
    'use strict';

    const configQueryMap = {
        'navbar-vertical-collapsed': 'phoenixIsNavbarVerticalCollapsed',
        'color-scheme': 'phoenixTheme',
        'navigation-type': 'phoenixNavbarPosition',
        'vertical-navbar-appearance': 'phoenixNavbarVerticalStyle',
        'horizontal-navbar-shape': 'phoenixNavbarTopShape',
        'horizontal-navbar-appearance': 'phoenixNavbarTopStyle'
    };

    const initialConfig = {
        phoenixIsNavbarVerticalCollapsed: false,
        phoenixTheme: 'light',
        phoenixNavbarTopStyle: 'default',
        phoenixNavbarVerticalStyle: 'default',
        phoenixNavbarPosition: 'horizontal', // <- Set horizontal navbar by default
        phoenixNavbarTopShape: 'default',
        phoenixIsRTL: false,
        phoenixSupportChat: false
    };

    const CONFIG = { ...initialConfig };

    const setConfig = (obj, persist = true) => {
        Object.keys(obj).forEach(key => {
            CONFIG[key] = obj[key];
    if (persist) {
        localStorage.setItem(key, obj[key]);
    }
});
};

const resetConfig = () => {
    Object.keys(initialConfig).forEach(key => {
        CONFIG[key] = initialConfig[key];
localStorage.setItem(key, initialConfig[key]);
});
};

const urlSearchParams = new URLSearchParams(window.location.search);
const params = Object.fromEntries(urlSearchParams.entries());

if (Object.keys(params).length > 0 && Object.keys(params).includes('theme-control')) {
    resetConfig();
    Object.keys(params).forEach(key => {
        if (configQueryMap[key]) {
        localStorage.setItem(configQueryMap[key], params[key]);
}
});
}

// 🔒 Force horizontal navbar as default (overrides any user-set config)
localStorage.setItem('phoenixNavbarPosition', 'horizontal');
document.documentElement.setAttribute('data-navigation-type', 'horizontal');

// Load and apply the rest of the config
Object.keys(CONFIG).forEach(key => {
    if (key !== 'phoenixNavbarPosition') {
    const storedValue = localStorage.getItem(key);
if (storedValue === null) {
    localStorage.setItem(key, CONFIG[key]);
} else {
    try {
        setConfig({ [key]: JSON.parse(storedValue) });
    } catch {
        setConfig({ [key]: storedValue });
    }
}
}
});

// Apply visual changes
if (JSON.parse(localStorage.getItem('phoenixIsNavbarVerticalCollapsed'))) {
    document.documentElement.classList.add('navbar-vertical-collapsed');
}

const theme = localStorage.getItem('phoenixTheme');
if (theme === 'dark') {
    document.documentElement.setAttribute('data-bs-theme', 'dark');
} else if (theme === 'auto') {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    document.documentElement.setAttribute('data-bs-theme', prefersDark ? 'dark' : 'light');
}

const navType = localStorage.getItem('phoenixNavbarPosition');
if (navType === 'horizontal') {
    document.documentElement.setAttribute('data-navigation-type', 'horizontal');
} else if (navType === 'combo') {
    document.documentElement.setAttribute('data-navigation-type', 'combo');
}

const config = {
    config: CONFIG,
    reset: resetConfig,
    set: setConfig
};

return config;
});
