import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faCircleCheck,
  faTriangleExclamation,
} from '@fortawesome/free-solid-svg-icons';
import { Container, Content } from 'react-bulma-components';

interface ConnectedFooterProps {
  isConnected: boolean;
}

export default function ConnectedFooter({ isConnected }: ConnectedFooterProps) {
  return (
    <Container>
      <Content style={{ textAlign: 'right', color: '$grey-light' }}>
        <h2>
          {isConnected ? 'Connected! ' : 'Not Connected! '}
          <FontAwesomeIcon
            color={isConnected ? 'green' : 'yellow'}
            icon={isConnected ? faCircleCheck : faTriangleExclamation}
          />
        </h2>
      </Content>
    </Container>
  );
}
