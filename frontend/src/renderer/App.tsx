import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import { useEffect, useState } from 'react';
import { Button, Icon, Navbar, Tabs } from 'react-bulma-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faGear } from '@fortawesome/free-solid-svg-icons';
import AppConstants from '../types/AppConstants';
import SettingsModal from '../components/SettingsModal';
import ConnectedFooter from '../components/ConnectedFooter';
import { ElectronHandler } from '../main/preload';
import { isApiConnected } from '../helpers/TypeSafety';
import PersonalFinanceTab from '../components/PersonalFinanceTab';

enum ActiveTabs {
  PersonalFinance = 'Personal Finance',
}

interface MainPageArgs {
  contextBridge: ElectronHandler;
}

function MainPage({ contextBridge }: MainPageArgs) {
  const [appConstants, setAppConstants] = useState<AppConstants>({
    apiUrl: null,
  });

  const [settingsWindowVisible, setSettingsWindowVisible] = useState(false);
  const [isConnected, setIsConnected] = useState(false);

  const [activeTab, setActiveTab] = useState<ActiveTabs>(
    ActiveTabs.PersonalFinance,
  );

  useEffect(() => {
    let timerRef: ReturnType<typeof setInterval> | undefined;

    if (appConstants.apiUrl !== undefined) {
      timerRef = setInterval(
        async () =>
          contextBridge.ipcRenderer
            .invoke('poll-online', appConstants.apiUrl)
            .then(
              (result) => {
                if (isApiConnected(result)) {
                  setIsConnected(result.connected);
                  console.log(result.message);
                }

                return null;
              },
              (err) => console.log(err),
            ),
        3000,
      );
    }

    return () => clearInterval(timerRef);
  }, [contextBridge, appConstants]);

  return (
    <div className="app-page">
      <SettingsModal
        isVisible={settingsWindowVisible}
        settingsCallback={(settings) => {
          setSettingsWindowVisible(false);
          if (settings !== undefined) {
            setAppConstants(settings);
            contextBridge.ipcRenderer.sendMessage('settings-change', settings);
          }
        }}
      />
      <Navbar color="primary">
        <Navbar.Brand>
          <Navbar.Item>Budget Tool</Navbar.Item>
        </Navbar.Brand>
        <Navbar.Menu>
          <Navbar.Container>
            <Navbar.Item>Stuff</Navbar.Item>
          </Navbar.Container>
          <Navbar.Container align="right">
            <Navbar.Item>
              <Button text={false} outlined={false}>
                <span>
                  <Icon onClick={() => setSettingsWindowVisible(true)}>
                    <FontAwesomeIcon icon={faGear} />
                  </Icon>
                </span>
              </Button>
            </Navbar.Item>
          </Navbar.Container>
        </Navbar.Menu>
      </Navbar>
      <Tabs type="boxed" align="center" className="nav-tabs">
        {Object.values(ActiveTabs).map((at) => (
          <Tabs.Tab active={activeTab === at} key={`tab-${at}`}>
            {at}
          </Tabs.Tab>
        ))}
      </Tabs>
      <div className="tab-content">
        {activeTab === 'Personal Finance' && (
          <PersonalFinanceTab
            contextBridge={contextBridge}
            isConnected={isConnected}
          />
        )}
      </div>
      <ConnectedFooter isConnected={isConnected} className="app-footer" />
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
