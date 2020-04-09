from requests import get, post, put, delete
import time
from testlib import order, BASE_URL, auth, GIRAFTestCase

guardian_token = ''
citizen_token = ''
citizen_username = f'Alice{time.time()}'
template_id = 0


class TestWeekTemplateController(GIRAFTestCase):
    """
    Testing API requests on WeekTemplate endpoints
    """

    @classmethod
    def setUpClass(cls) -> None:
        """
        Setup necessary data when class is loaded
        """
        super(TestWeekTemplateController, cls).setUpClass()
        print(f'file:/{__file__}\n')
        cls.TEMPLATES = [{'thumbnail': {'id': 28}, 'name': 'Template1', 'days':
                         [{'day': 'Monday', 'activities': [{'pictogram': {'id': 1}, 'order': 0, 'state': 'Active'},
                                                           {'pictogram': {'id': 6}, 'order': 0, 'state': 'Active'}]},
                         {'day': 'Friday', 'activities': [{'pictogram': {'id': 2}, 'order': 0, 'state': 'Active'},
                                                          {'pictogram': {'id': 7}, 'order': 0, 'state': 'Active'}]}]},
                         {'thumbnail': {'id': 29}, 'name': 'Template2', 'days':
                         [{'day': 'Monday', 'activities': [{'pictogram': {'id': 2}, 'order': 1, 'state': 'Active'},
                                                           {'pictogram': {'id': 7}, 'order': 2, 'state': 'Active'}]},
                          {'day': 'Friday', 'activities': [{'pictogram': {'id': 3}, 'order': 1, 'state': 'Active'},
                                                           {'pictogram': {'id': 8}, 'order': 2, 'state': 'Active'}]}]}]

    @classmethod
    def tearDownClass(cls) -> None:
        """
        Remove or resolve necessary data and states after class tests are done
        """
        super(TestWeekTemplateController, cls).tearDownClass()

    @order
    def test_week_template_can_login_as_guardian(self):
        """
        Testing logging in as Guardian

        Endpoint: POST:/v1/Account/login
        """
        global guardian_token
        data = {'username': 'Graatand', 'password': 'password'}
        response = post(f'{BASE_URL}v1/Account/login', json=data).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        guardian_token = response['data']

    @order
    def test_week_template_can_register_citizen(self):
        """
        Testing registering Citizen

        Endpoint: POST:/v1/Account/register
        """
        data = {'username': citizen_username, 'password': 'password', 'role': 'Citizen', 'departmentId': 1}
        response = post(f'{BASE_URL}v1/Account/register', headers=auth(guardian_token), json=data).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')

    @order
    def test_week_template_can_login_as_citizen(self):
        """
        Testing logging in as Citizen

        Endpoint: POST:/v1/Account/login
        """
        global citizen_token
        data = {'username': citizen_username, 'password': 'password'}
        response = post(f'{BASE_URL}v1/Account/login', json=data).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        citizen_token = response['data']

    @order
    def test_week_template_can_get_all_templates(self):
        """
        Testing getting all templates

        Endpoint: GET:/v1/WeekTemplate
        """
        response = get(f'{BASE_URL}v1/WeekTemplate', headers=auth(guardian_token)).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        self.assertEqual('SkabelonUge', response['data'][0]['name'])
        self.assertEqual(1, response['data'][0]['templateId'])

    @order
    def test_week_template_can_get_specific_template(self):
        """
        Testing getting specific template

        Endpoint: GET:/v1/WeekTemplate/{id}
        """
        response = get(f'{BASE_URL}v1/WeekTemplate/1', headers=auth(guardian_token)).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        self.assertEqual('SkabelonUge', response['data']['name'])
        self.assertEqual(1, response['data']['thumbnail']['id'])
        self.assertEqual(1, response['data']['days'][0]['day'])
        self.assertEqual(6, response['data']['days'][5]['day'])
        self.assertEqual(70, response['data']['days'][4]['activities'][1]['pictogram']['id'])

    @order
    def test_week_template_can_get_template_outside_department_should_fail(self):
        """
        Testing getting template from outside department

        Endpoint: GET:/v1/WeekTemplate/{id}
        """
        response = get(f'{BASE_URL}v1/WeekTemplate/1', headers=auth(citizen_token)).json()
        self.assertFalse(response['success'])
        self.assertEqual(response['errorKey'], 'NotAuthorized')

    @order
    def test_week_template_can_add_new_template(self):
        """
        Testing adding new template

        Endpoint: POST:/v1/WeekTemplate
        """
        global template_id
        response = post(f'{BASE_URL}v1/WeekTemplate', headers=auth(guardian_token), json=self.TEMPLATES[0]).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        template_id = response['data']['id']

    @order
    def test_week_template_ensure_template_is_added(self):
        """
        Testing ensuring template has been added

        Endpoint: GET:/v1/WeekTemplate/{id}
        """
        response = get(f'{BASE_URL}v1/WeekTemplate/{template_id}', headers=auth(guardian_token)).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        self.assertEqual(28, response['data']['thumbnail']['id'])
        self.assertEqual(6, response['data']['days'][0]['activities'][1]['pictogram']['id'])
        self.assertEqual(7, response['data']['days'][1]['activities'][1]['pictogram']['id'])

    @order
    def test_week_template_can_update_template(self):
        """
        Testing updating template

        Endpoint: PUT:/v1/WeekTemplate/{id}
        """
        response = put(f'{BASE_URL}v1/WeekTemplate/{template_id}', headers=auth(guardian_token),
                       json=self.TEMPLATES[1]).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')

    @order
    def test_week_template_ensure_template_is_updated(self):
        """
        Testing ensuring template has been updated

        Endpoint: GET:/v1/WeekTemplate/{id}
        """
        response = get(f'{BASE_URL}v1/WeekTemplate/{template_id}', headers=auth(guardian_token)).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')
        self.assertIsNotNone(response['data'])
        self.assertEqual(29, response['data']['thumbnail']['id'])
        self.assertEqual(7, response['data']['days'][0]['activities'][1]['pictogram']['id'])
        self.assertEqual(8, response['data']['days'][1]['activities'][1]['pictogram']['id'])

    @order
    def test_week_template_can_delete_template(self):
        """
        Testing deleting template

        Endpoint: DELETE:/v1/WeekTemplate/{id}
        """
        response = delete(f'{BASE_URL}v1/WeekTemplate/{template_id}', headers=auth(guardian_token)).json()
        self.assertTrue(response['success'])
        self.assertEqual(response['errorKey'], 'NoError')

    @order
    def test_week_template_ensure_template_is_deleted(self):
        """
        Testing ensuring template has been deleted

        Endpoint: GET:/v1/WeekTemplate/{id}
        """
        response = get(f'{BASE_URL}v1/WeekTemplate/{template_id}', headers=auth(guardian_token)).json()
        self.assertFalse(response['success'])
        self.assertEqual(response['errorKey'], 'NoWeekTemplateFound')