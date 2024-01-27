import { Button, Columns, Form, Modal } from 'react-bulma-components';
import { useState } from 'react';
import AppConstants from '../types/AppConstants';

interface SettingsModalProps {
  isVisible: boolean;
  settingsCallback: (constants: AppConstants | undefined) => void;
}

export default function SettingsModal({
  isVisible,
  settingsCallback,
}: SettingsModalProps) {
  const [apiUri, setApiUri] = useState('');

  return (
    <Modal show={isVisible}>
      <Modal.Card>
        <Modal.Card.Header>
          <Modal.Card.Title>Settings</Modal.Card.Title>
        </Modal.Card.Header>
        <Modal.Card.Body>
          <Form.Field>
            <Form.Label>API URI</Form.Label>
            <Form.Control>
              <Form.Input
                value={apiUri}
                onChange={(event) => setApiUri(event.target.value)}
              />
            </Form.Control>
            <Form.Help>The URI of your GRPC API</Form.Help>
          </Form.Field>
        </Modal.Card.Body>
        <Modal.Card.Footer>
          <Columns>
            <Columns.Column>
              <Button
                color="danger"
                onClick={() => settingsCallback(undefined)}
              >
                Cancel
              </Button>
            </Columns.Column>
            <Columns.Column alignContent="end">
              <Button
                color="confirm"
                onClick={() => settingsCallback({ apiUrl: apiUri })}
              >
                Save
              </Button>
            </Columns.Column>
          </Columns>
        </Modal.Card.Footer>
      </Modal.Card>
    </Modal>
  );
}
