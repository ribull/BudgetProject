// Disable no-unused-vars, broken for spread args
/* eslint no-unused-vars: off */
import { contextBridge, ipcRenderer, IpcRendererEvent } from 'electron';

export type Channels =
  | 'poll-online'
  | 'settings-change'
  | 'upload-file'
  | 'get-purchases'
  | 'add-purchase'
  | 'edit-purchase'
  | 'delete-purchase'
  | 'add-category'
  | 'get-categories'
  | 'get-pay-histories'
  | 'add-pay-history'
  | 'edit-pay-history'
  | 'delete-pay-history'
  | 'get-eras'
  | 'add-era'
  | 'edit-era'
  | 'delete-era'
  | 'get-future-purchases'
  | 'add-future-purchase'
  | 'edit-future-purchase'
  | 'delete-future-purchase'
  | 'get-investments'
  | 'add-investment'
  | 'edit-investment'
  | 'delete-investment'
  | 'get-savings'
  | 'add-saved'
  | 'edit-saved'
  | 'delete-saved'
  | 'get-wishlist'
  | 'add-wishlist-item'
  | 'edit-wishlist-item'
  | 'delete-wishlist-item';

const electronHandler = {
  ipcRenderer: {
    sendMessage(channel: Channels, ...args: unknown[]) {
      ipcRenderer.send(channel, ...args);
    },
    on(channel: Channels, func: (...args: unknown[]) => void) {
      const subscription = (_event: IpcRendererEvent, ...args: unknown[]) =>
        func(...args);
      ipcRenderer.on(channel, subscription);

      return () => {
        ipcRenderer.removeListener(channel, subscription);
      };
    },
    once(channel: Channels, func: (...args: unknown[]) => void) {
      ipcRenderer.once(channel, (_event, ...args) => func(...args));
    },
    invoke(channel: Channels, ...args: unknown[]): Promise<any> {
      return ipcRenderer.invoke(channel, ...args);
    },
  },
};

contextBridge.exposeInMainWorld('electron', electronHandler);

export type ElectronHandler = typeof electronHandler;
