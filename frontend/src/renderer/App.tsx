import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import 'bulma/css/bulma.min.css';
import './App.css';
import { useEffect, useState } from 'react';
import { Icon, Level, Navbar } from 'react-bulma-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { icon } from '@fortawesome/fontawesome-svg-core/import.macro';
import InsertPurchase from '../components/InsertPurchase';
import AppConstants from '../types/AppConstants';
import SettingsModal from '../components/SettingsModal';
import ConnectedFooter from '../components/ConnectedFooter';
import { ElectronHandler } from '../main/preload';
import isBoolean from '../helpers/TypeSafety';

interface MainPageArgs {
  contextBridge: ElectronHandler;
}

function MainPage({ contextBridge }: MainPageArgs) {
  const [appConstants, setAppConstants] = useState<AppConstants>({
    apiUrl: undefined,
  });

  const [settingsWindowVisible, setSettingsWindowVisible] = useState(false);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    let timerRef: ReturnType<typeof setInterval> | undefined;

    if (appConstants.apiUrl !== undefined) {
      timerRef = setInterval(
        async () =>
          contextBridge.ipcRenderer
            .invoke('poll-online', appConstants.apiUrl)
            .then((result) => {
              if (isBoolean(result)) {
                setIsConnected(result);
              }

              return null;
            })
            .catch((err) => console.log(err)),
        3000,
      );
    }

    return () => clearInterval(timerRef);
  }, [contextBridge, appConstants]);

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        alignContent: 'space-between',
      }}
    >
      <SettingsModal
        isVisible={settingsWindowVisible}
        settingsCallback={(settings) => {
          setSettingsWindowVisible(false);
          if (settings !== undefined) {
            setAppConstants(settings);
          }
        }}
      />
      <Navbar style={{ flexGrow: 0 }}>
        <Navbar.Brand>Budget Tool</Navbar.Brand>
        <Navbar.Menu>
          <Navbar.Container>Stuff</Navbar.Container>
          <Navbar.Container align="right">
            <Icon onClick={() => setSettingsWindowVisible(true)}>
              <FontAwesomeIcon
                icon={icon({ name: 'gear', family: 'classic', style: 'solid' })}
              />
            </Icon>
          </Navbar.Container>
        </Navbar.Menu>
      </Navbar>
      <div style={{ flexGrow: 1 }} />
      <ConnectedFooter style={{ flexGrow: 0 }} isConnected={isConnected} />
    </div>
  );
}

export default function App({ contextBridge }: MainPageArgs) {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<MainPage contextBridge={contextBridge} />} />
      </Routes>
    </Router>
  );
}
