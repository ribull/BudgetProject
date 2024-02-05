/* eslint global-require: off, no-console: off, promise/always-return: off */

/**
 * This module executes inside of electron's main process. You can start
 * electron renderer process from here and communicate with the other processes
 * through IPC.
 *
 * When running `npm run build` or `npm run build:main`, this file is compiled to
 * `./src/main.js` using webpack. This gives us some performance wins.
 */
import path from 'path';
import { app, BrowserWindow, shell, ipcMain } from 'electron';
import { autoUpdater } from 'electron-updater';
import log from 'electron-log';
import { ChannelCredentials } from '@grpc/grpc-js';
import MenuBuilder from './menu';
import { resolveHtmlPath } from './util';
import { BudgetServiceClient } from '../generated/budget_service';
import { HealthClient } from '../generated/health_check';
import { GRPC_PORT, REST_PORT } from '../helpers/Constants';
import {
  addCategory,
  addPurchase,
  getCategories,
  getPurchases,
  pollOnline,
  uploadFile,
} from './contextBridgeFunctions';
import { isAppConstants } from '../helpers/TypeSafety';

class AppUpdater {
  constructor() {
    log.transports.file.level = 'info';
    autoUpdater.logger = log;
    autoUpdater.checkForUpdatesAndNotify();
  }
}

let mainWindow: BrowserWindow | null = null;
let budgetService: BudgetServiceClient | null = null;
let healthCheckService: HealthClient | null = null;
let serverName: string | null = null;

// Events
ipcMain.on('settings-change', (event, arg) => {
  if (isAppConstants(arg)) {
    if (arg.apiUrl !== null) {
      serverName = arg.apiUrl;
      budgetService = new BudgetServiceClient(
        `${serverName}:${GRPC_PORT}`,
        ChannelCredentials.createInsecure(),
      );

      healthCheckService = new HealthClient(
        `${serverName}:${GRPC_PORT}`,
        ChannelCredentials.createInsecure(),
      );
    }
  }
});

ipcMain.handle('poll-online', async () => pollOnline(healthCheckService));

ipcMain.handle('get-purchases', async (event, arg) =>
  getPurchases(arg, budgetService),
);

ipcMain.handle('add-purchase', async (event, args) =>
  addPurchase(args, budgetService),
);

ipcMain.handle('upload-file', async (event, arg) =>
  uploadFile(arg, `${serverName}:${REST_PORT}`),
);

ipcMain.handle('get-categories', async () => getCategories(budgetService));

ipcMain.handle('add-category', async (event, arg) =>
  addCategory(arg, budgetService),
);

if (process.env.NODE_ENV === 'production') {
  const sourceMapSupport = require('source-map-support');
  sourceMapSupport.install();
}

const isDebug =
  process.env.NODE_ENV === 'development' || process.env.DEBUG_PROD === 'true';

if (isDebug) {
  require('electron-debug')();
}

const installExtensions = async () => {
  const installer = require('electron-devtools-installer');
  const forceDownload = !!process.env.UPGRADE_EXTENSIONS;
  const extensions = ['REACT_DEVELOPER_TOOLS'];

  return installer
    .default(
      extensions.map((name) => installer[name]),
      forceDownload,
    )
    .catch(console.log);
};

const createWindow = async () => {
  if (isDebug) {
    await installExtensions();
  }

  const RESOURCES_PATH = app.isPackaged
    ? path.join(process.resourcesPath, 'assets')
    : path.join(__dirname, '../../assets');

  const getAssetPath = (...paths: string[]): string => {
    return path.join(RESOURCES_PATH, ...paths);
  };

  mainWindow = new BrowserWindow({
    show: false,
    icon: getAssetPath('icon.png'),
    webPreferences: {
      preload: app.isPackaged
        ? path.join(__dirname, 'preload.js')
        : path.join(__dirname, '../../.erb/dll/preload.js'),
    },
  });

  mainWindow.loadURL(resolveHtmlPath('index.html'));

  mainWindow.on('ready-to-show', () => {
    if (!mainWindow) {
      throw new Error('"mainWindow" is not defined');
    }
    if (process.env.START_MINIMIZED) {
      mainWindow.minimize();
    } else {
      mainWindow.maximize();
    }
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  const menuBuilder = new MenuBuilder(mainWindow);
  menuBuilder.buildMenu();

  // Open urls in the user's browser
  mainWindow.webContents.setWindowOpenHandler((edata) => {
    shell.openExternal(edata.url);
    return { action: 'deny' };
  });

  // Remove this if your app does not use auto updates
  // eslint-disable-next-line
  new AppUpdater();
};

/**
 * Add event listeners...
 */

app.on('window-all-closed', () => {
  // Respect the OSX convention of having the application in memory even
  // after all windows have been closed
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app
  .whenReady()
  .then(() => {
    createWindow();
    app.on('activate', () => {
      // On macOS it's common to re-create a window in the app when the
      // dock icon is clicked and there are no other windows open.
      if (mainWindow === null) createWindow();
    });
  })
  .catch(console.log);
